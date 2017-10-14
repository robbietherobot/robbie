using Microsoft.ProjectOxford.Vision;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Microsoft.ProjectOxford.Vision.Contract;

namespace RobbieSenses.Evaluation
{
    /// <summary>
    /// Class implementing the actual computer vision functionality based on the Microsoft Cognitive Services Vision API.
    /// </summary>
    public class ComputerVision
    {
        /// <summary>
        /// The actual Microsoft Vision API client.
        /// </summary>
        private readonly VisionServiceClient visionServiceClient;

        /// <summary>
        /// Initializes the computer vision based on the application resources configuration.
        /// </summary>
        public ComputerVision()
        {
            var resources = ResourceLoader.GetForCurrentView("/RobbieSenses/Resources");
            var visionApiKey = resources.GetString("VisionAPIKey");

            visionServiceClient = new VisionServiceClient(visionApiKey);
        }

        /// <summary>
        /// Gets an array of tags seen in the give image.
        /// </summary>
        /// <param name="memoryStream">The memory stream of the image to analyse.</param>
        /// <returns>A list of Tag objects describing the image in the memory stream.</returns>
        public async Task<Tag[]> GetTags(MemoryStream memoryStream)
        {
            var analysis = await visionServiceClient.GetTagsAsync(memoryStream);

            return analysis.Tags;
        }
    }
}
