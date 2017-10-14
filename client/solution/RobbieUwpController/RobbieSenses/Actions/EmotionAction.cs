using RobbieSenses.Interfaces;

namespace RobbieSenses.Actions
{
    /// <summary>
    /// Emotion action implementation, to let Robbie show a certain emotion.
    /// </summary>
    public class EmotionAction: IAction
    {
        /// <summary>
        /// Gets the emotion of the current action.
        /// </summary>
        public string Emotion { get; }

        /// <summary>
        /// Constructs a new emotion action.
        /// </summary>
        /// <param name="emotion">The emotion to show.</param>
        public EmotionAction(string emotion)
        {
            Emotion = emotion;
        }
    }
}
