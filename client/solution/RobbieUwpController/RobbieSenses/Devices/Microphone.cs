using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace RobbieSenses.Devices
{
    /// <summary>
    /// Camera utility class handling all camera operations for the different senses.
    /// </summary>
    /// <remarks>Note that this class uses the Singleton pattern to make sure there's only one camera handle for the entire application.</remarks>
    public class Microphone
    {
        /// <summary>
        /// The default media encoding profile used to store recorded fragments in.
        /// </summary>
        private readonly MediaEncodingProfile wavEncodingProfile;

        /// <summary>
        /// Local instance of the MediaCapture class for capturing audio from the microphone.
        /// </summary>
        private MediaCapture mediaCapture;

        /// <summary>
        /// In memory random access stream buffer for recording audio fragments.
        /// </summary>
        private InMemoryRandomAccessStream buffer;

        /// <summary>
        /// The single instance of the Microphone class used by the Singleton pattern.
        /// </summary>
        private static Microphone instance;

        private Microphone()
        {
            wavEncodingProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
            wavEncodingProfile.Audio = AudioEncodingProperties.CreatePcm(16000, 1, 16);

            Initialize();
        }

        /// <summary>
        /// Retrieve the Singleton instance of the Camera class, creating a new object if necessary.
        /// </summary>
        public static Microphone Instance => instance ?? (instance = new Microphone());

        /// <summary>
        /// Initializes the MediaCapture element and its event handlers.
        /// </summary>
        private async void Initialize()
        {
            try
            {
                var settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                };
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync(settings);

                // define the event handlers for handling errors
                mediaCapture.Failed += mediaCapture_Failed;
                mediaCapture.RecordLimitationExceeded += mediaCapture_RecordLimitExceeded;
            }
            catch (Exception ex)
            {
                // if speech recording is not yet allowed on the device, an unauthorized access exception will be thrown
                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(UnauthorizedAccessException))
                {
                    // throw the inner exception to expose the real exception and disclose more relevant information
                    throw ex.InnerException;
                }
                throw;
            }
        }

        /// <summary>
        /// Record a fragment of the given duration.
        /// </summary>
        /// <param name="duration">The desired duration in milliseconds.</param>
        /// <returns>The recorded fragmet as a RandomAccessStream object.</returns>
        public async Task<IRandomAccessStream> Record(int duration)
        {
            buffer?.Dispose();
            buffer = new InMemoryRandomAccessStream();
            await mediaCapture.StartRecordToStreamAsync(wavEncodingProfile, buffer);

            await Task.Delay(duration);

            await mediaCapture.StopRecordAsync();
            return buffer.CloneStream();
        }

        /// <summary>
        /// Handles the MediaCapture Failed event. Currently not (yet) implemented.
        /// </summary>
        /// <param name="currentCaptureObject">The MediaCapture object that failed to capture the audio from the microphone.</param>
        /// <param name="currentFailure">The event args object of the failed capture.</param>
        private void mediaCapture_Failed(MediaCapture currentCaptureObject, MediaCaptureFailedEventArgs currentFailure)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the event of exceeding the record limit of the MediaCapture object. Currently not (yet) implemented.
        /// </summary>
        /// <param name="currentCaptureObject">The MediaCapture object that failed to capture the audio from the microphone.</param>
        public void mediaCapture_RecordLimitExceeded(MediaCapture currentCaptureObject)
        {
            throw new NotImplementedException();
        }
    }
}
