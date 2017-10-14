using System;

namespace RobbieSpinalCord.Models
{
    /// <summary>
    /// Profile response after identification.
    /// </summary>
    public class IdentifyResponse
    {
        /// <summary>
        /// The name of the person as known by Sitecore.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The gender of the person.
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// The day of birth of the person.
        /// </summary>
        public DateTime? BirthDate { get; set; }
    }
}
