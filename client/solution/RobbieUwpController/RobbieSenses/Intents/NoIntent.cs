using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS;
using RobbieSpinalCord.Interfaces;
using RobbieSenses.Actions;
using System.Collections.Generic;
using RobbieSenses.Interfaces;

namespace RobbieSenses.Intents
{
    /// <summary>
    /// Implementation of the 'no intent recognized' case.
    /// </summary>
    public class NoIntent : IntentBase
    {
        /// <summary>
        /// Constructs a new no intent object.
        /// </summary>
        /// <param name="intent">The LuisResult containing the detected intent.</param>
        /// <param name="client">The Client for which the intent has been detected.</param>
        public NoIntent(LuisResult intent, IClient client) : base(intent, client)
        {
        }

        /// <summary>
        /// Returns the response for the 'no intent recognized' case.
        /// </summary>
        /// <remarks>While this specific intent handler isn't awaiting any calls, the intent handler always calls his intents asynchronously.</remarks>
        /// <returns>A list of actions for Robbie to execute, based on the current intent.</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<IList<IAction>> HandleIntent()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var action = new NoIntentAction();
            Actions.Add(action);
            return Actions;
        }
    }
}
