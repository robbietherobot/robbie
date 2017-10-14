using Sitecore.Data;

namespace Robbie.xDB.Extensions.Services
{
    public interface ITrackerService
    {
        void IdentifyContact(string identifier);
        void TrackOutcome(ID definitionId);
        void TrackPageEvent(ID pageEventItemId);
        bool IsActive { get; }
    }
}
