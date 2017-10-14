using Sitecore.Analytics.Model.Entities;
using Sitecore.Analytics.Tracking;

namespace Robbie.xDB.Providers
{
    public interface IContactProfileProvider
    {
        IContactPersonalInfo PersonalInfo { get; }
        IContactAddresses Addresses { get; }
        IContactEmailAddresses Emails { get; }
        IContactCommunicationProfile CommunicationProfile { get; }
        IContactPhoneNumbers PhoneNumbers { get; }
        Contact Flush();
        Contact Contact { get; }
        IContactPicture Picture { get; }
        IContactPreferences Preferences { get; }
        Sitecore.Analytics.Tracking.KeyBehaviorCache KeyBehaviorCache { get; }
    }
}
