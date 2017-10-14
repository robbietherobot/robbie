using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face;
using RobbieSenses.Devices;
using RobbieSenses.Evaluation;
using RobbieSenses.Interfaces;
using RobbieSenses.Output;
using RobbieSenses.Visualization;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace RobbieSenses.Input
{
    /// <summary>
    /// Handles the eyes, both the input (Camera) and output (EyesDisplay (LED Matrix)), to be able to see and express emotions.   
    /// </summary>
    public class Eyes : IEyes
    {
        /// <summary>
        /// Face tracking instance.
        /// </summary>
        private readonly FaceTracking faceTracking;

        /// <summary>
        /// Face detection instance.
        /// </summary>
        private readonly FaceDetection faceDetection;

        /// <summary>
        /// Computer vision instance.
        /// </summary>
        private readonly ComputerVision computerVision;

        /// <summary>
        /// Emotion detection instance.
        /// </summary>
        private readonly EmotionDetection emotionDetection;

        /// <summary>
        /// IdentityInterpolation object used to keep track of all identities and corresponding personal data harvested by other API calls.
        /// </summary>
        private readonly IdentityInterpolation identityInterpolation;

        /// <summary>
        /// The canvas element used for rendering the image preview to, showing what Robbie sees.
        /// </summary>
        private readonly Canvas previewCanvas;
        
        /// <summary>
        /// Vision object, a utility processing the visualization of what Robbie sees.
        /// </summary>
        private readonly Vision visualization;

        /// <summary>
        /// PanTilt instance steering the neck movement of Robbie (both vertical and horizontal).
        /// </summary>
        private readonly PanTilt panTilt;

        /// <summary>
        /// The Eyes Display object controlling the eyes output.
        /// </summary>
        private readonly EyesDisplay eyesDisplay;          

        /// <summary>
        /// Robbie's sleeping state, keeping track if Robbie is sleeping or awake.
        /// </summary>
        private bool sleeping;

        /// <summary>
        /// Semaphore used for throttling the frame capturing process.
        /// </summary>
        private readonly SemaphoreSlim frameProcessingSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Delegate used for the NewActivePerson event
        /// </summary>
        /// <param name="identity">The tracked identity currently having the largest face, indicating this is new active person.</param>
        public delegate void NewActivePersonEventHandler(TrackedIdentity identity);

        /// <summary>
        /// Event fired when a new active person has been identified
        /// </summary>
        public event NewActivePersonEventHandler NewActivePersonEvent;

        /// <summary>
        /// used for training of faces
        /// </summary>
        private string lastFaceName;

        /// <summary>
        /// holds current identity
        /// </summary>
        private TrackedIdentity currentIdentity;

        /// <summary>
        /// Constructs a new eyes object.
        /// </summary>
        /// <param name="visionPreview">A capture element that is placed on a canvas used for capturing what Robbie sees.</param>
        /// <param name="previewCanvas">A canvas element used for rendering the image preview showing what Robbie sees.</param>
        public Eyes(CaptureElement visionPreview, Canvas previewCanvas)
        {
            Camera.Instance.Initialize(visionPreview);
            this.previewCanvas = previewCanvas;

            faceTracking = new FaceTracking();
            faceDetection = new FaceDetection();
            computerVision = new ComputerVision();
            emotionDetection = new EmotionDetection();

            identityInterpolation = new IdentityInterpolation();
            visualization = new Vision();
            panTilt = new PanTilt();
            eyesDisplay = new EyesDisplay();

            identityInterpolation.LargestFaceChanged += IdentityInterpolation_LargestFaceChanged;

            // fire up the continuous tasks of processing video and controlling the servos
            ThreadPoolTimer.CreatePeriodicTimer(ProcessCurrentVideoFrame_Delegate, TimeSpan.FromMilliseconds(125)); // 8 fps
            ThreadPoolTimer.CreatePeriodicTimer(UpdatePanTiltPosition_Delegate, TimeSpan.FromMilliseconds(25)); // 40 fps
            ThreadPoolTimer.CreatePeriodicTimer(Blink_Delegate, TimeSpan.FromMilliseconds(5000)); // 12 fpm            
        }
        
        /// <summary>
        /// Called when Robbie wakes up, disabling sleep mode and setting his emotion to neutral.
        /// </summary>
        public void WakeUp()
        {
            sleeping = false;
            eyesDisplay.SetEmotion(EyesDisplay.Emotions.Neutral); 
        }

        /// <summary>
        /// Called when Robbie goes in hibernate mode, enabling sleep mode and closes his lids.
        /// </summary>
        public void Hibernate()
        {
            sleeping = true;
            eyesDisplay.SetEmotion(EyesDisplay.Emotions.Sleep);
        }

        /// <summary>
        /// Gets the name of the current identity, or null if no identity is currently tracked.
        /// </summary>
        public string CurrentIdentityName
        {
            get
            {
                return currentIdentity?.Name;
            }
        }

        /// <summary>
        /// Callback handler of the largest face changed event of the identity interpolation.
        /// This event handler is used for the brain to switch the active session with the server to the person Robbie is currently interacting with.
        /// </summary>
        /// <param name="personId">The person ID of the identity currently having the largest face.</param>
        private async void IdentityInterpolation_LargestFaceChanged(Guid personId)
        {
            // don't respond to this event while sleeping
            if (sleeping) return;
            
            await IdentifyFaces();
        }

        /// <summary>
        /// Scan the current video frame for faces, identifying and storing them.
        /// </summary>
        public async Task IdentifyFaces()
        {
            await Camera.Instance.CapturePhoto(IdentifyFaces_Delegate);            
        }

        /// <summary>
        /// Delegate to define the actions taken upon the photo capture of the camera class.
        /// This one uses the memory stream of the photo capture to identify the faces in the iamge, using the local face detection instance.
        /// </summary>
        /// <param name="memoryStream">The memory stream containing the captured image.</param>
        private async Task IdentifyFaces_Delegate(MemoryStream memoryStream)
        {
            var faces = await faceDetection.DetectFaces(memoryStream);

            if (faces.Length > 0)
            {
                // you could call the face API once for all faces together
                // but there's no way to map the found persons to the detected face rectangles
                // so we have to call the API per detected face rectangle
                foreach (var face in faces)
                {
                    try
                    {
                        var persons = await faceDetection.IdentifyFace(new[] {face});

                        // set identities when there is at least one person within the viewport
                        if (persons.Count > 0)
                        {
                            var person = persons.FirstOrDefault();
                            if (person != null)
                            {
                                identityInterpolation.IdentifiedFace(face, person);
                            }
                        }

                        // remove the current identity when there is no person identified
                        else if (persons.Count == 0)
                        {
                            currentIdentity = null;
                        }
                    }
                    catch (FaceAPIException)
                    {
                        // if the person group is not yet trained or face identification fails for any other reason, continue
                    }
                }

                // after adding new identified faces, switch to interact with the possibly new person within the viewport,
                // so make sure to get the new largest face object (event won't be fired again!)
                var trackedIdentity = identityInterpolation.GetLargestFace();
                if (trackedIdentity != null)
                {
                    // if there is no current identity, handle the new person as found
                    if(currentIdentity == null)
                    {
                        OnPersonFound(trackedIdentity);
                    }
                    else if(currentIdentity != null)
                    {
                        // if there is an identity currently, check if it has actually changed before calling person found 
                        if (currentIdentity.PersonId != trackedIdentity.PersonId)
                        {
                            OnPersonFound(trackedIdentity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when a new main person to interact with has been found / appeared within the viewport.
        /// The current identity will be set to this identity and the corresponding event will be fired, so the brain can subscribe to / act upon this event.
        /// </summary>
        /// <param name="trackedIdentity">The new TrackedIdentity to interact with.</param>
        private void OnPersonFound(TrackedIdentity trackedIdentity)
        {
            currentIdentity = trackedIdentity;

            var handler = NewActivePersonEvent;
            // ReSharper disable once UseNullPropagation
            if (handler != null)
            {
                handler(currentIdentity);
            }
        }

        /// <summary>
        /// Create a new person within the Face API to track.
        /// </summary>
        /// <returns>The newly created person ID for this person.</returns>
        public async Task<Guid> CreatePerson()
        {
            return await faceDetection.CreatePerson(new Guid().ToString());
        }

        /// <summary>
        /// Update the name of a person within the Face API, used to set the name of a person after getting to know his name.
        /// You can only call this method once!
        /// </summary>
        /// <remarks>
        /// Mind that you cannot call this multiple times, because this method attaches the given name and the person ID to each other (within the FaceDetection class).
        /// </remarks>
        /// <param name="personId">The person ID within the Face API of the person to update the name of.</param>
        /// <param name="name">The new name to set.</param>
        /// <returns>The new name of the person.</returns>
        public async Task<string> UpdatePerson(Guid personId, string name)
        {
            return await faceDetection.UpdatePersonName(personId, name);
        }
        
        /// <summary>
        /// Stores the current image capture containing the face of a person for training purposes.
        /// </summary>
        /// <param name="name">The name of the person to store the face for.</param>
        public async Task StoreFaceFor(string name)
        {
            lastFaceName = name;              
            await Camera.Instance.CapturePhoto(StoreFace_Delegate);
        }

        /// <summary>
        /// Delegate to define the actions taken upon the photo capture of the camera class.
        /// This one uses the memory stream of the photo capture to store the face in the image, using the local face detection instance.  
        /// </summary>
        /// <param name="memoryStream">The memory stream containing the captured image.</param>
        private async Task StoreFace_Delegate(MemoryStream memoryStream)
        {
            // if the last face name isn't set, return to avoid exceptions
            if (string.IsNullOrEmpty(lastFaceName)) return;

            // store the image of the current face for the given name
            if (await faceDetection.StoreFaceFor(lastFaceName, memoryStream))
            {
                await TrainFaces();
            }
        }

        /// <summary>
        /// Trains the person group, which should be called after storing new faces or new face data.
        /// </summary>
        /// <returns>A Task object for this is an asynchronous method.</returns>
        public async Task TrainFaces()
        {
            await faceDetection.TrainFaces();
        }

        /// <summary>
        /// Tells the eyes to show a certain emotion.
        /// </summary>
        /// <param name="emotion">The emotion string matching one of the available emotions (will be Neutral when not recognized).</param>
        public void ShowEmotion(string emotion)
        {
            eyesDisplay.SetEmotion(emotion);
        }

        /// <summary>
        /// Get the emotions of the current identity (if there is a current identity).
        /// </summary>
        /// <returns>An EmotionScores object describing the emotions of the current identity.</returns>
        public async Task<EmotionScores> GetEmotions()
        {
            if (currentIdentity == null) return null;

            await RecognizeEmotions();
            return currentIdentity.EmotionScores;                        
        }

        /// <summary>
        /// Scan the current video frame for emotions, attaching them to the currently stored identities.
        /// </summary>
        public async Task RecognizeEmotions()
        {
            await Camera.Instance.CapturePhoto(RecognizeEmotions_Delegate);
        }

        /// <summary>
        /// Delegate to define the actions taken upon the photo capture of the camera class.
        /// This one uses the memory stream of the photo capture to detect emotions in the given image, using the local computer vision instance.  
        /// </summary>
        /// <param name="memoryStream">The memory stream containing the captured image.</param>
        private async Task RecognizeEmotions_Delegate(MemoryStream memoryStream)
        {
            var emotions = await emotionDetection.DetectEmotions(memoryStream);

            if (emotions != null && emotions.Length > 0)
            {
                foreach (var emotion in emotions)
                {
                    identityInterpolation.DetectedEmotion(emotion);
                }
            }
        }

        /// <summary>
        /// Tells what Robbie sees.
        /// </summary>
        public async Task WhatDoYouSee()
        {
            await Camera.Instance.CapturePhoto(WhatDoYouSee_Delegate);
        }

        /// <summary>
        /// Delegate to define the actions taken upon the photo capture of the camera class.
        /// This one uses the memory stream of the photo capture to look for tags in the given image, using the local computer vision instance.  
        /// </summary>
        /// <param name="memoryStream">The memory stream containing the captured image.</param>
        private async Task WhatDoYouSee_Delegate(MemoryStream memoryStream)
        {
            var tags = await computerVision.GetTags(memoryStream);

            if (tags.Length == 0)
            {
                // todo: no objects found, handle this scenario
            }
            
            // todo: objects found in vision, handle this scenario, looping through the recognized objects and basing any actions upon this information
        }

        /// <summary>
        /// Delegate to process the latest or current video frame, used for tracking faces.
        /// </summary>
        /// <param name="timer">The timer object used by the TimeElapsedHandler delegate.</param>
        private async void ProcessCurrentVideoFrame_Delegate(ThreadPoolTimer timer)
        {
            if (!frameProcessingSemaphore.Wait(0))
            {
                return;
            }

            var currentFrame = await Camera.Instance.GetLatestFrame();

            try
            {
                faceTracking.Track(currentFrame);

                // currentFrame sometimes throws an null reference exception
                var bitmap = currentFrame.SoftwareBitmap;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    identityInterpolation.Update(faceTracking.DetectedFaces);

                    panTilt.FocalPoint = identityInterpolation.GetFocalPoint();
                    visualization.DecorateScreenCapture(previewCanvas, bitmap, identityInterpolation.Identities, panTilt.FocalPoint);
                });
            }
            catch (Exception)
            {
                // ignored because it isn't an issue when a frame drops
            }
            finally
            {
                frameProcessingSemaphore.Release();
            }
        }

        /// <summary>
        /// Delegate to update the pan tilt position via a periodic timer.
        /// </summary>
        /// <param name="timer">The timer object used by the TimeElapsedHandler delegate.</param>
        private void UpdatePanTiltPosition_Delegate(ThreadPoolTimer timer)
        {
            if (!sleeping)
            {
                panTilt.MoveTowardsFocalPoint();
            }
        }

        /// <summary>
        /// Lets the eyes blink once.
        /// </summary>
        private void Blink_Delegate(ThreadPoolTimer timer)
        {
            if (!sleeping)
            {
                eyesDisplay.Blink();
            }
        }

        /// <summary>
        /// Disposes the eyes display, turning of the LEDs. 
        /// </summary>
        public void Dispose()
        {
            eyesDisplay.Dispose();
        }
    }
}
