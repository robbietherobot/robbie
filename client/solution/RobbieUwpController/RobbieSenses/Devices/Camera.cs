using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Controls;

namespace RobbieSenses.Devices
{
    /// <summary>
    /// Camera utility class handling all camera operations for the different senses.
    /// </summary>
    /// <remarks>Note that this class uses the Singleton pattern to make sure there's only one camera handle for the entire application.</remarks>
    public class Camera
    {
        /// <summary>
        /// Local instance of the MediaCapture class for capturing photos and video (in this case) from the webcam device.
        /// </summary>
        private MediaCapture mediaCapture;

        /// <summary>
        /// A preview frame of the camera, used for tracking and other continuous operations.
        /// </summary>
        private VideoFrame previewFrame;

        /// <summary>
        /// Indicates whether the preview is active (if the Camera is initialized and the CaptureElement is passed to it).
        /// Currently only in use to know what to clean when disposing the camera object.
        /// </summary>
        private bool isPreviewing;

        /// <summary>
        /// Indicates whether the camera is currently capturing a frame, to prevent duplicate (concurrent) calls to a capturing delegate. 
        /// </summary>
        private bool isCapturing;

        /// <summary>
        /// The single instance of the Camera class used by the Singleton pattern.
        /// </summary>
        private static volatile Camera instance;
        
        /// <summary>
        /// Object instance used by the double-check lock pattern to lock onto, instead of locking on the instance itself, to avoid deadlocks.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The size of the viewport set by the SetPreviewFrame method.
        /// </summary>
        private Size viewPortSize; 

        /// <summary>
        /// Camera constructor.
        /// </summary>
        private Camera()
        {
        }

        /// <summary>
        /// Retrieve the Singleton instance of the Camera class, creating a new object if necessary.
        /// </summary>
        public static Camera Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (instance == null)
                            instance = new Camera();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Returns the current viewport size.
        /// </summary>
        public Size ViewPortSize
        {
            get
            {
                return viewPortSize;
            }
        }

        /// <summary>
        /// Initialize the MediaCapture object and start the previewing process if applicable.
        /// </summary>
        /// <param name="captureElement">The CaptureElement to stream the Camera preview to (although the element can be set invisible, but we need it for  tracking).</param>
        public async void Initialize(CaptureElement captureElement)
        {
            // initialize the mediacapture object
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();

            // define the event handlers for handling errors
            mediaCapture.Failed += mediaCapture_Failed;
            mediaCapture.RecordLimitationExceeded += mediaCapture_RecordLimitExceeded;

            // bind the capture element to the media capture object to enable previewing and consequently, face tracking
            captureElement.Source = mediaCapture;
            await mediaCapture.StartPreviewAsync();
            isPreviewing = true;

            // define the preview frame for tracking purposes
            SetPreviewFrame();
        }

        /// <summary>
        /// Defines the preview frame used for face tracking purposes. The frame defined here, stored locally,
        /// functions as an input parameter for GetPreviewFrameAsync, which needs a video frame to copy the results into.
        /// </summary>
        /// <remarks>
        /// If the resource values configuring the preview frame size are set to "default", or parsing them to an integer value fails,
        /// the default media stream properties of the devices are used (which probably is the maximum resolution).
        /// </remarks>
        private void SetPreviewFrame()
        {
            var resources = ResourceLoader.GetForCurrentView("/RobbieSenses/Resources");
            var videoPreviewWidth = resources.GetString("VideoPreviewWidth");
            var videoPreviewHeight = resources.GetString("VideoPreviewHeight");

            var previewProperties =
                mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as
                    VideoEncodingProperties;

            if (previewProperties == null) return;

            int width, height;
            if (videoPreviewWidth.Equals("default", StringComparison.CurrentCultureIgnoreCase) ||
                !int.TryParse(videoPreviewWidth, out width))
            {
                width = (int) previewProperties.Width;
            }
            if (videoPreviewHeight.Equals("default", StringComparison.CurrentCultureIgnoreCase) ||
                !int.TryParse(videoPreviewHeight, out height))
            {
                height = (int) previewProperties.Height;
            }

            // store the calculated size for referencing outside this class
            viewPortSize.Width = width;
            viewPortSize.Height = height;

            // create the preview frame using the correct size and format
            previewFrame = new VideoFrame(BitmapPixelFormat.Nv12, width, height);
        }

        /// <summary>
        /// Delegate used to inject different methods of handling the result of the photo capture by the MediaCapture object.
        /// </summary>
        /// <param name="memoryStream">The stream containing the captured photo data.</param>
        public delegate Task CapturedImageAction(MemoryStream memoryStream);

        /// <summary>
        /// Delegate used to inject different methods of handling the result of the bitmap capture by the MediaCapture object.
        /// </summary>
        /// <param name="softwareBitmap">The bitmap containing the captured photo data.</param>
        public delegate void CapturedBitmapAction(SoftwareBitmap softwareBitmap);
        
        /// <summary>
        /// Captures a still image from the webcam as a stream, handing the result over to the supplied action delegate.
        /// </summary>
        /// <param name="capturedImageAction">A CapturedImageAction delegate to handle the captured image data.</param>
        public async Task CapturePhoto(CapturedImageAction capturedImageAction)
        {
            if (isCapturing) return;

            isCapturing = true;

            var properties = ImageEncodingProperties.CreateJpeg();
            var lowLagCapture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(properties);

            var capturedPhoto = await lowLagCapture.CaptureAsync();

            var stream = capturedPhoto.Frame.AsStreamForRead();
            var reader = new BinaryReader(stream);

            var byteCount = Convert.ToInt32(capturedPhoto.Frame.Size);
            var bytes = reader.ReadBytes(byteCount);
            var memoryStream = new MemoryStream(bytes);

            await capturedImageAction(memoryStream);

            await lowLagCapture.FinishAsync();

            isCapturing = false;
        }

        /// <summary>
        /// Captures a still image from the webcam as a bitmap, handing the result over to the supplied action delegate.
        /// </summary>
        /// <param name="capturedBitmapAction">A CapturedBitmapAction delegate to handle the captured image data.</param>
        public async void CaptureSoftwareBitmap(CapturedBitmapAction capturedBitmapAction)
        {
            if (isCapturing) return;

            isCapturing = true;

            var properties = ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Nv12);
            var lowLagCapture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(properties);

            var capturedPhoto = await lowLagCapture.CaptureAsync();

            var softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;
            capturedBitmapAction(softwareBitmap);

            await lowLagCapture.FinishAsync();

            isCapturing = false;
        }

        /// <summary>
        /// Returns the latest preview frame of the media capture element as a VideoFrame object.
        /// </summary>
        /// <returns>A VideoFrame object of the preview frame.</returns>
        public async Task<VideoFrame> GetLatestFrame()
        {
            VideoFrame latestFrame = null;
            if (previewFrame != null)
            {
                latestFrame = await mediaCapture.GetPreviewFrameAsync(previewFrame);
            }
            return latestFrame;
        }

        /// <summary>
        /// Dispose the MediaCapture object and the previewing if in previewing mode.
        /// </summary>
        public async void Dispose()
        {
            if (mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                    isPreviewing = false;
                }
                mediaCapture.Dispose();
                mediaCapture = null;
            }

            previewFrame?.Dispose();
        }

        /// <summary>
        /// Handles the MediaCapture Failed event. Currently not (yet) implemented.
        /// </summary>
        /// <param name="currentCaptureObject">The MediaCapture object that failed to capture the still image from the webcam.</param>
        /// <param name="currentFailure">The event args object of the failed capture.</param>
        private void mediaCapture_Failed(MediaCapture currentCaptureObject, MediaCaptureFailedEventArgs currentFailure)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the event of exceeding the record limit of the MediaCapture object. Currently not (yet) implemented.
        /// </summary>
        /// <param name="currentCaptureObject">The MediaCapture object that failed to capture the still image from the webcam.</param>
        public void mediaCapture_RecordLimitExceeded(MediaCapture currentCaptureObject)
        {
            throw new NotImplementedException();
        }
    }
}
