using RobbieSenses.Interfaces;

namespace RobbieSenses.Actions
{
    /// <summary>
    /// Say action implementation, to let Robbie speak.
    /// </summary>
    public class SayAction:IAction
    {
        /// <summary>
        /// Gets the reply (text to be spoken) of the current action.
        /// </summary>
        public string Reply { get; }

        /// <summary>
        /// Constructs a new say action.
        /// </summary>
        /// <param name="reply">The text to say.</param>
        public SayAction(string reply)
        {
            Reply = reply;
        }
    }
}
