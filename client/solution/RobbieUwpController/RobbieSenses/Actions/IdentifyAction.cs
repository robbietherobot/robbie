using RobbieSenses.Interfaces;

namespace RobbieSenses.Actions
{
    /// <summary>
    /// Identify action implementation, when Robbie identifies someone.
    /// </summary>
    public class IdentifyAction: IAction
    {
        /// <summary>
        /// Gets the person ID of the current action.
        /// </summary>
        public string PersonId { get; }

        /// <summary>
        /// Constructs a new identify action.
        /// </summary>
        /// <param name="personId">The ID of the identified person.</param>
        public IdentifyAction(string personId)
        {
            PersonId = personId;
        }
    }
}
