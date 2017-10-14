namespace RobbieBehaviour.Models
{
    /// <summary>
    /// View model object used to update the profile.
    /// </summary>
    public class UpdateProfileViewModel
    {
        /// <summary>
        /// Name to update (name is always sent to FirstName field).
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Lastname to update (currently not used).
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Age to update.
        /// </summary>
        public double Age { get; set; }

        /// <summary>
        /// Emotion to update.
        /// todo: emotion is a more complex object, containing multiple emotions of different intensities (but isn't this already taken care of? or in other words: can be removed?)
        /// </summary>
        public string Emotion { get; set; }

        /// <summary>
        /// Gender to update.
        /// </summary>
        public string Gender { get; set; }
    }
}