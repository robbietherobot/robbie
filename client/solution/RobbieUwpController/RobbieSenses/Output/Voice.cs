using System;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;
using RobbieSenses.Interfaces;
using Windows.UI.Core;

namespace RobbieSenses.Output
{
    /// <summary>
    /// Class handling all speech synthesize related features.
    /// </summary>
    public class Voice : IVoice
    {
        /// <summary>
        /// The media element used to play spoken text with.
        /// </summary>
        private readonly MediaElement mediaElement;

        /// <summary>
        /// The speech synthesizer object used to generate the speech from text.
        /// </summary>
        private readonly SpeechSynthesizer synthesizer;

        /// <summary>
        /// Event signaling the playback of the speech audio fragment has finished.
        /// </summary>
        public event FinishedPlayback FinishedPlaybackEventHandler;

        /// <summary>
        /// Constructs a voice object.
        /// </summary>
        /// <param name="audioPlaybackElement">The media element to playback audio with for the voice synthesizing.</param>
        /// <remarks>Note that the given media element should be placed on a canvas in order to raise events and thus work properly.</remarks>
        public Voice(MediaElement audioPlaybackElement)
        {
            mediaElement = audioPlaybackElement;
            mediaElement.MediaEnded += MediaElement_MediaEnded;
            synthesizer = new SpeechSynthesizer();
        }

        /// <summary>
        /// Speech synthesis implementation to let Robbie say the given text.
        /// </summary>
        /// <param name="text">Text to say.</param>
        public async Task Say(string text)
        {
            // the media element must be placed on a canvas to be able to raise these event(s)            
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await Play(text);
            });
        }

        /// <summary>
        /// Plays the audio stream for a given text. 
        /// </summary>
        /// <param name="text">The text to play as a stream.</param>
        /// <returns>Returns a task object for this method is asynchronous.</returns>
        private async Task Play(string text)
        {
            var stream = await GetAudioStream(text);
            PlayStream(stream);
        }

        /// <summary>
        /// Plays the SpeechSynthesis stream.
        /// </summary>
        /// <param name="stream">The stream to play.</param>
        private void PlayStream(SpeechSynthesisStream stream)
        {
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }
        
        /// <summary>
        /// Gets the synthesisstream for a given text
        /// </summary>
        /// <param name="text">text to synthesize</param>
        /// <returns></returns>
        private async Task<SpeechSynthesisStream> GetAudioStream(string text)
        {            
            var results = await synthesizer.SynthesizeTextToStreamAsync(text);
            return results;
        }

        /// <summary>
        /// Handles the MediaEnded event. Only works when the MediaElement is placed on a canvas.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Routed event arguments object.</param>
        private void MediaElement_MediaEnded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var handler = FinishedPlaybackEventHandler;
            // ReSharper disable once UseNullPropagation
            if(handler != null)
            {
                handler();
            }
        }
    }  
}
