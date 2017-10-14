using Sitecore.Foundation.Accounts.Providers;
using System;

namespace RobbieBehaviour.Models
{
    /// <summary>
    /// Profile view model used to return profile data to Robbie.
    /// </summary>
    public class ProfileViewModel
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

        /// <summary>
        /// Public constructor to create a Profile based on the current contact info.
        /// </summary>
        /// <param name="contactProfileProvider">The contact profile provider object containing the required PersonalInfo object.</param>
        public ProfileViewModel(IContactProfileProvider contactProfileProvider)
        {
            this.Name = contactProfileProvider.PersonalInfo.FirstName;
            this.Gender = contactProfileProvider.PersonalInfo.Gender;
            this.BirthDate = contactProfileProvider.PersonalInfo.BirthDate;   
        }
    }
}