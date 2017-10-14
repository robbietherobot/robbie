using System.IO;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Emotion;
using Windows.ApplicationModel.Resources;
using Microsoft.ProjectOxford.Emotion.Contract;

namespace RobbieSenses.Evaluation
{
    /// <summary>
    /// Class implementing the actual emotion detection based on the Microsoft Cognitive Services Emotion API.
    /// </summary>
    public class EmotionDetection
    {
        /// <summary>
        /// The actual Microsoft Emotion API client.
        /// </summary>
        private readonly EmotionServiceClient emotionServiceClient;


        /// <summary>
        /// Initializes the emotion detection based on the application resources configuration.
        /// </summary>
        public EmotionDetection()
        {
            var resources = ResourceLoader.GetForCurrentView("/RobbieSenses/Resources");
            var emotionApiKey = resources.GetString("EmotionAPIKey");

            emotionServiceClient = new EmotionServiceClient(emotionApiKey);
        }

        /// <summary>
        /// Detects emotions of all faces within the given image stream.
        /// </summary>
        /// <param name="imageStream">The stream of the image to detect the emotions in.</param>
        /// <returns>An array of Emotion objects, describing the emotions found.</returns>
        public async Task<Emotion[]> DetectEmotions(MemoryStream imageStream)
        {
            return await emotionServiceClient.RecognizeAsync(imageStream);
        }
    }
}
