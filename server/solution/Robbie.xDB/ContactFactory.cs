using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Analytics;
using Sitecore.Analytics.Model;
using Sitecore.Analytics.Data;
using Sitecore.Analytics.Tracking;
using Sitecore.Configuration;
using Sitecore.Analytics.DataAccess;
using Sitecore.Analytics.Model.Entities;

namespace Robbie.xDB
{
    public class ContactFactory
    {
        // based on https://jonathanrobbins.co.uk/2016/01/20/how-to-identify-and-merge-contacts-in-sitecore-xdb/
        // and https://jonathanrobbins.co.uk/2016/01/20/how-to-update-contacts-in-sitecore-xdb/

        private readonly ContactRepository contactRepository;
        private readonly ContactManager contactManager;

        public ContactFactory()
        {
            contactRepository = Factory.CreateObject("tracking/contactRepository", true) as ContactRepository;
            contactManager = Factory.CreateObject("tracking/contactManager", true) as ContactManager;
        }

        public Contact GetContact(string identifier)
        {
            if (IsContactInSession(identifier))
            {
                return Tracker.Current.Session.Contact;
            }

            var matchedContact = Tracker.Current.Session.Contact;

            var contact = contactRepository.LoadContactReadOnly(identifier);
            if (contact != null)
            {
                LockAttemptResult<Contact> lockResult = contactManager.TryLoadContact(contact.ContactId);
                switch (lockResult.Status)
                {
                    case LockAttemptStatus.Success:
                        Contact lockedContact = lockResult.Object;
                        lockedContact.ContactSaveMode = ContactSaveMode.AlwaysSave;
                        matchedContact = lockedContact;
                        break;
                }
            }

            if (!matchedContact.ContactId.Equals(Tracker.Current.Session.Contact.ContactId))
            {
                Tracker.Current.Session.Identify(identifier);
            }

            return matchedContact;
        }

        private bool IsContactInSession(string identifier)
        {
            var tracker = Tracker.Current;

            return tracker != null &&
                   tracker.IsActive &&
                   tracker.Session?.Contact?.Identifiers?.Identifier != null
                   &&
                   tracker.Session.Contact.Identifiers.Identifier.Equals(identifier,
                       StringComparison.InvariantCultureIgnoreCase);
        }


        public void SetPersonalData(Contact contact, string firstName, string gender, double age)
        {
            var contactPersonalInfo = contact.GetFacet<IContactPersonalInfo>("Personal");

            contactPersonalInfo.FirstName = firstName;
            contactPersonalInfo.Gender = gender;

            var days = (int)Math.Round(age * 365.242); // estimate
            var ageTimeSpan = new TimeSpan(days, 0, 0, 0);
            contactPersonalInfo.BirthDate = DateTime.Now.Subtract(ageTimeSpan);
        }

        public void SetProfilePicture(Contact contact, byte[] picture)
        {
            var contactPicture = contact.GetFacet<IContactPicture>("Picture");
            contactPicture.Picture = picture;
        }

        public void ReleaseAndSave(Contact contact)
        {
            contactManager.SaveAndReleaseContactToXdb(contact);
        }
    }
}
