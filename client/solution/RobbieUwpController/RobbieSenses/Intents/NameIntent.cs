using Microsoft.Cognitive.LUIS;
using System.Threading.Tasks;
using RobbieSpinalCord.Interfaces;
using RobbieSenses.Actions;
using RobbieSenses.Interfaces;
using System.Collections.Generic;

namespace RobbieSenses.Intents
{
    /// <summary>
    /// Name intent handling implementation.
    /// </summary>
    public class NameIntent : IntentBase
    {
        /// <summary>
        /// Constructs a new name intent object.
        /// </summary>
        /// <param name="intent">The LuisResult containing the detected intent.</param>
        /// <param name="client">The Client for which the intent has been detected.</param>
        public NameIntent(LuisResult intent, IClient client) : base(intent, client)
        {

        }

        /// <summary>
        /// Updates the profile name and returns a reply.
        /// </summary>
        /// <remarks>While this specific intent handler isn't awaiting any calls, the intent handler always calls his intents asynchronously.</remarks>
        /// <returns>A list of actions for Robbie to execute, based on the current intent.</returns>
#pragma warning disable 1998
        public override async Task<IList<IAction>> HandleIntent()
#pragma warning restore 1998
        {                       
            const string entityName = "Name";

            var name = string.Empty;
            if (Intent.Entities.ContainsKey(entityName))
            {
                var entities = Intent.Entities;
                var entity = entities[entityName];
                name = entity[0].Value;
            }

            var action = new NameAction(name);

            Actions.Add(action);
            return Actions; 
        }
    }
}
