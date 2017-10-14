using System;

namespace RobbieSpinalCord.Models
{
    /// <summary>
    /// Profile that will be used in Brains (should be moved over in a later stage)
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// The name of the person.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The gender of the person.
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// The (estimated) age of the person.
        /// </summary>
        public double Age { get; set; }

        /// <summary>
        /// The day of birth of the person.
        /// </summary>
        public DateTime? BirthDate { get; set; }

        // todo: emotion is a more complex object, containing multiple emotions of different intensities (but isn't this already taken care of? or in other words: can be removed?)
        public string Emotion { get; set; }

        // todo: seems to be unused, is this for future use?
        public string VoiceId { get; set; }

        // todo: seems to be unused, is this for future use?
        public string AnalyticsId { get; set; }        
    }
}
