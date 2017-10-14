using RobbieSenses.Interfaces;

namespace RobbieSenses.Actions
{
    /// <summary>
    /// Command action implementation, to let Robbie respond to pre-configured commands.
    /// </summary>
    public class CommandAction : IAction
    {
        /// <summary>
        /// Gets the command of the current action.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Constructs a new command action.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public CommandAction(string command)
        {
            Command = command;
        }
    }
}
