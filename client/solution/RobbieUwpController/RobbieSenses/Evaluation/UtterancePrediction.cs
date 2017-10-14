using Microsoft.Cognitive.LUIS;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using RobbieSenses.Interfaces;
using System;

namespace RobbieSenses.Evaluation
{
    /// <summary>
    /// Class implementing the LUIS utterance prediction functionality based on the Microsoft Cognitive LUIS API.
    /// </summary>
    public class UtterancePrediction : IUtterance
    {
        /// <summary>
        /// The actual Microsoft LUIS client.
        /// </summary>
        private readonly LuisClient luisClient;

        /// <summary>
        /// Spoken text command that can be used to wake Robbie up.
        /// </summary>
        public const string WakeUpCommand = "wake up";

        /// <summary>
        /// Spoken text command that can be used to hibernate Robbie.
        /// </summary>
        public const string HibernateCommand = "sleep";

        /// <summary>
        /// Spoken text command that can be used to quit Robbie.
        /// </summary>
        public const string QuitCommand = "shut down";

        /// <summary>
        /// Constructs a new utterance prediction object, initializing a LUIS Client to predict utterances.
        /// </summary>
        public UtterancePrediction()
        {
            var resources = ResourceLoader.GetForCurrentView("/RobbieSenses/Resources");
            var luisAppId = resources.GetString("LUISAppID");
            var luisAppKey = resources.GetString("LUISAppKey");

            luisClient = new LuisClient(luisAppId, luisAppKey);
        }

        /// <summary>
        /// Gets the predicted intent based on the given utterance.
        /// </summary>
        /// <param name="utterance">An utterance string to predict the intent of.</param>
        /// <returns>A LUIS result object containing the predicted intent.</returns>
        public async Task<LuisResult> GetIntent(string utterance)
        {
            // first check if any hard coded commands match the given utterance
            if (utterance.Equals(HibernateCommand, StringComparison.CurrentCultureIgnoreCase))
            {
                return GetIntentForCommand(HibernateCommand);
            }

            if (utterance.Equals(WakeUpCommand, StringComparison.CurrentCultureIgnoreCase))
            {
                return GetIntentForCommand(WakeUpCommand);
            }

            if (utterance.Equals(QuitCommand, StringComparison.CurrentCultureIgnoreCase))
            {
                return GetIntentForCommand(QuitCommand);
            }

            // otherwise get Luis to predict what the utterance means
            return await luisClient.Predict(utterance);            
        }

        /// <summary>
        /// Create a new LuisResult object based on the given command.
        /// </summary>
        /// <param name="command">The command text to create the intent result for.</param>
        /// <returns>A LuisResult object descrining the intent that goes with the given command.</returns>
        private LuisResult GetIntentForCommand(string command)
        {
            var intent = new LuisResult
            {
                TopScoringIntent = new Intent
                {
                    Name = command,
                    Score = 1
                }
            };
            return intent;
        }
    }
}
