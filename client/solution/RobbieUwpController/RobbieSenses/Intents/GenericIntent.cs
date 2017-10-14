using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS;
using RobbieSpinalCord.Interfaces;
using RobbieSenses.Actions;
using System.Collections.Generic;
using RobbieSenses.Interfaces;

namespace RobbieSenses.Intents
{
    /// <summary>
    /// Generic intent handling implementation.
    /// </summary>
    public class GenericIntent : IntentBase
    {
        /// <summary>
        /// Constructs a new generic intent object.
        /// </summary>
        /// <param name="intent">The LuisResult containing the detected intent.</param>
        /// <param name="client">The Client for which the intent has been detected.</param>
        public GenericIntent(LuisResult intent, IClient client) : base(intent, client)
        {
        }

        /// <summary>
        /// Retrieves the personalized reply from the server.
        /// </summary>
        /// <returns>A list of actions for Robbie to execute, based on the current intent.</returns>
        public override async Task<IList<IAction>> HandleIntent()
        {
            var reply = await Client.GetIntentAction(Intent.TopScoringIntent.Name);

            var sayAction = new SayAction(reply.Reply);
            var emoteAction = new EmotionAction(reply.Emotion);            

            Actions.Add(emoteAction);
            Actions.Add(sayAction);

            return Actions;
        }
    }
}
