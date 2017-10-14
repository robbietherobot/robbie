using RobbieSenses.Interfaces;

namespace RobbieSenses.Actions
{
    /// <summary>
    /// Name action implementation, for when an anonymous user is named.
    /// </summary>
    public class NameAction : IAction
    {
        /// <summary>
        /// Gets the name of the current action.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Constructs a new name action.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        public NameAction(string name)
        {
            Name = name;
        }
    }
}
