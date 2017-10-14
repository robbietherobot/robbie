using RobbieSenses.Devices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.SpeechRecognition;
using Windows.Storage.Streams;
using RobbieSenses.Evaluation;
using RobbieSenses.Interfaces;

namespace RobbieSenses.Input
{
    /// <summary>
    /// Enumeration describing all states the Ears of Robbie could be in.
    /// </summary>
    public enum EarsState
    {
        NotInitialized,
        Initialized,
        Idle,
        StartListening,
        Listening,
        StopListening,
        StoppedListening,
        Processing            
    }    

    /// <summary>
    /// Class controlling all microphone related task, like recording audio fragments or hearing text phrases.
    /// </summary>
    public class Ears : IEars
    {
        /// <summary>
        /// Speech recognition object used for speech to text interpretation.
        /// </summary>
        private SpeechRecognition speechRecognition;
        
        /// <summary>
        /// The current state of the Ears object.
        /// </summary>
        public EarsState State { get; private set; }

        /// <summary>
        /// The event to subscribe to if you want to be notified when the ears state changes.
        /// </summary>
        public event EarsStateChangedHandler EarsStateChanged;        

        /// <summary>
        /// The event to subscribe to if you want to process recognized utterances.
        /// </summary>
        public event UtteranceRecognizedHandler SpeechRecognized;        

        /// <summary>
        /// Constructs a new Ears object.
        /// </summary>
        public Ears()
        {
            ChangeState(EarsState.NotInitialized);
            Initialize();
        }

        /// <summary>
        /// Initializes the speech recognition object.
        /// </summary>
        public async void Initialize()
        {
            speechRecognition = new SpeechRecognition();
            if (await speechRecognition.Initialize())
            {
                ChangeState(EarsState.Initialized);
            }
        }

        /// <summary>
        /// If properly initialized, starts the speech recognition process and binds the SpeechProcessed event.
        /// </summary>
        /// <remarks>Note that after an utterance has been recognized, the listener must be started explicitly again.</remarks>
        public void StartListening()
        {
            if (State == EarsState.Initialized || State == EarsState.Idle || State == EarsState.StoppedListening)
            {
                ChangeState(EarsState.StartListening);
                speechRecognition.StartListening(speech_Processed);
                ChangeState(EarsState.Listening);
            }
        }

        /// <summary>
        /// Changes the current state and notifies listeners.
        /// </summary>
        /// <param name="state">The new state to set the ears to.</param>
        private void ChangeState(EarsState state)
        {
            State = state;

            var handler = EarsStateChanged;
            // ReSharper disable once UseNullPropagation
            if (handler != null)
            {
                handler(state);
            }
        }
        
        /// <summary>
        /// Stops the listening process. You can use this if you want to interrupt listening (for example, when Robbie is speaking himself).
        /// </summary>
        public void StopListening()
        {
            ChangeState(EarsState.StopListening);
            speechRecognition.StopListening();
            ChangeState(EarsState.StoppedListening);
        }

        /// <summary>
        /// Handles the speech processed event and fires the SpeechRecognizedEvent when text has been recognized with some confidence.
        /// </summary>
        /// <param name="asyncInfo">The recognizer task containing the speech recognition results.</param>
        /// <param name="asyncStatus">The status of the recognition process.</param>
        /// <remarks>Note that after an utterance has been recognized, the listener must be started explicitly again.</remarks>
        private void speech_Processed(IAsyncOperation<SpeechRecognitionResult> asyncInfo, AsyncStatus asyncStatus)
        {
            ChangeState(EarsState.Processing);
            if (asyncInfo.Status == AsyncStatus.Completed)
            {
                var results = asyncInfo.GetResults();
                if (results.Confidence != SpeechRecognitionConfidence.Rejected)
                {
                    var text = results.Text;

                    var handler = SpeechRecognized;
                    // ReSharper disable once UseNullPropagation
                    if (handler != null)
                    {
                        handler(text);
                    }
                    ChangeState(EarsState.Idle);
                }
                else
                {
                    // if the speech recognition result is completed, but the result is rejected, start listening again!
                    ChangeState(EarsState.StoppedListening);
                    StartListening();
                }            
            }
            if (asyncStatus == AsyncStatus.Canceled)
            {
                ChangeState(EarsState.Idle);
            }                        
        }

        /// <summary>
        /// Returns an audio fragment, recorded by the microphone, as a RandomAccessStream object, of the given length in milliseconds.
        /// </summary>
        /// <param name="duration">The desired duration of the recording; 3 seconds by default.</param>
        /// <returns>A RandomAccessStream object containing the recorded audio.</returns>
        public Task<IRandomAccessStream> GetAudio(int duration = 3000)
        {
            return Microphone.Instance.Record(duration);
        }
    }
}
