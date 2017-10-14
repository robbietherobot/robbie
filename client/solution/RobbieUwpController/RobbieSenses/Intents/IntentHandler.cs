using Microsoft.Cognitive.LUIS;
using System.Threading.Tasks;
using RobbieSenses.Evaluation;
using RobbieSenses.Interfaces;
using RobbieSpinalCord.Interfaces;
using System.Collections.Generic;

namespace RobbieSenses.Intents
{
    /// <summary>
    /// Determines the topscoring intent and creates the corresponding handling logic based on the topscoring intent.
    /// </summary>
    public class IntentHandler
    {
        private readonly LuisResult intent;
        private readonly IClient client;
        private readonly string topScoringIntent;

        /// <summary>
        /// Score threshold for distinguishing top scoring intents.
        /// </summary>
        private const double ScoreThreshold = 0.50d;

        /// <summary>
        /// Constructs a new intent handler specifically containing logic for the given intent.
        /// </summary>
        /// <param name="intent">The LuisResult containing the detected intent.</param>
        /// <param name="client">The Client for which the intent has been detected.</param>
        public IntentHandler(LuisResult intent, IClient client)
        {
            this.intent = intent;
            this.client = client;
            topScoringIntent = GetTopScoringIntent();
        }

        /// <summary>
        /// Handles the intent based on the applicable handler / logic.
        /// </summary>
        /// <returns>A list of actions for Robbie to execute, based on the detected intent.</returns>
        public async Task<IList<IAction>> HandleIntent()
        {
            // if an intent has been recognized, get the correct implementation for it
            var intentHandler = IntentFactory();
            var actions = await intentHandler.HandleIntent();            

            return actions;
        }

        /// <summary>
        /// Returns the applicable intent handler for the specific intent. If no specific intent handling logic exists, it defaults back to generic intent handling logic.
        /// </summary>
        /// <returns>The intent handler for the current intent.</returns>
        private IIntent IntentFactory()
        {
            IIntent intentHandler;
            var topScore = topScoringIntent.ToLower();
            switch (topScore)
            {                                
                case UtterancePrediction.WakeUpCommand:                
                case UtterancePrediction.HibernateCommand:
                case UtterancePrediction.QuitCommand:
                    intentHandler = new CommandIntent(intent, topScore);
                    break;
                case "name":
                    intentHandler = new NameIntent(intent, client);
                    break;
                case "none":
                    intentHandler = new NoIntent(intent, client);
                    break;
                default:
                    intentHandler = new GenericIntent(intent, client);
                    break;
            }            

            return intentHandler;
        }

        /// <summary>
        /// Determines the top scoring intent. 
        /// </summary>
        /// <returns>The name of the top scoring intent.</returns>
        private string GetTopScoringIntent()
        {
            var result = "none";
            if (intent?.TopScoringIntent != null && intent.TopScoringIntent.Score > ScoreThreshold)
            {
                result = intent.TopScoringIntent.Name;
            }            

            return result;
        }
    }
}
    