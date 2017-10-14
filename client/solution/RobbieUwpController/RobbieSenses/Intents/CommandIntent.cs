using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS;
using RobbieSenses.Actions;
using System.Collections.Generic;
using RobbieSenses.Interfaces;

namespace RobbieSenses.Intents
{
    /// <summary>
    /// Command intent handling implementation.
    /// </summary>
    public class CommandIntent : IntentBase
    {
        private readonly string command;
        /// <summary>
        /// Constructs a new intent handler specifically containing logic for the given intent.
        /// </summary>
        /// <param name="intent">The LuisResult containing the detected intent.</param>
        /// <param name="topScoringIntent">The top scoring intent used instead of the raw topScoringIntent data of the LuisResult object, to be able to steer threshold for example.</param>
        public CommandIntent(LuisResult intent, string topScoringIntent) : base(intent)
        {
            // the topScoringIntent is further calculated within the IntentHandler and gets priority over the raw topScoringIntent data within the LuisResult object
            command = topScoringIntent;
        }

        /// <summary>
        /// Returns the command for this command intent, used to program specific (hard coded) commands.
        /// Command intents do not use Luis for utterance interpretation and only respond to the exact voice command matching the command name and text.
        /// </summary>
        /// <remarks>While this specific intent handler isn't awaiting any calls, the intent handler always calls his intents asynchronously.</remarks>
        /// <returns>A list of actions for Robbie to execute, based on the current intent.</returns>
#pragma warning disable 1998
        public override async Task<IList<IAction>> HandleIntent()
#pragma warning restore 1998
        {
            var commandAction = new CommandAction(command);
            Actions.Add(commandAction);            
            return Actions;
            
        }
    }
}
