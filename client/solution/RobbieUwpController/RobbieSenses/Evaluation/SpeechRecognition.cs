using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.SpeechRecognition;

namespace RobbieSenses.Evaluation
{
    /// <summary>
    /// Class implementing the speech recognition functionality based on the Windows SpeechRecognition namespace.
    /// </summary>
    public class SpeechRecognition
    {
        /// <summary>
        /// The actual speech recognition object of the Windows SpeechRecognition namespace.
        /// </summary>
        private SpeechRecognizer speechRecognizer;

        /// <summary>
        /// The recognize task used for recognizing each new utterance.
        /// </summary>
        private IAsyncOperation<SpeechRecognitionResult> recognizeTask;
        
        /// <summary>
        /// Initializes a SpeechRecognition object, configuring the recognizer and grammar.
        /// </summary>
        /// <returns>True if the initialization process succeeded.</returns>
        public async Task<bool> Initialize()
        {
            speechRecognizer = new SpeechRecognizer(SpeechRecognizer.SystemSpeechLanguage);

            var webSearchGrammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.WebSearch, "webSearch");

            speechRecognizer.Constraints.Add(webSearchGrammar);

            var compilationResult = await speechRecognizer.CompileConstraintsAsync();
            return compilationResult.Status == SpeechRecognitionResultStatus.Success;
        }

        /// <summary>
        /// Starts the recognition process by creating a new recognize task, attaching the supplied event handler for processing the result.
        /// </summary>
        /// <param name="speechProcessedEventHandler">The event handler to call when a utterance is recongized.</param>
        public void StartListening(Action<IAsyncOperation<SpeechRecognitionResult>, AsyncStatus> speechProcessedEventHandler)
        {
            recognizeTask = speechRecognizer.RecognizeAsync();
            recognizeTask.Completed += speechProcessedEventHandler.Invoke;
        }

        /// <summary>
        /// Stops the recognition process if not yet idle.
        /// </summary>
        public async void StopListening()
        {
            if (recognizeTask != null && recognizeTask.Status == AsyncStatus.Started)
            {
                recognizeTask.Cancel();
            }

            if (speechRecognizer != null && speechRecognizer.State != SpeechRecognizerState.Idle)
            {
                await speechRecognizer.StopRecognitionAsync();
            }
        }
    }
}
