using RobbieSpinalCord.Actions;
using RobbieSpinalCord.Models;
using System.Threading.Tasks;

namespace RobbieSpinalCord.Interfaces
{
    public interface IClient
    {
        string PersonId { get; set; }
        Task<IdentifyResponse> Identify();
        Task<IntentAction> GetIntentAction(string intent);
        Task<IdentifyResponse> GetProfile();
        Profile Login(string voiceId);
        Task<string> UpdateProfile(Profile p);
        Task<ExperienceModel> UpdateProfileEmotions(ProfileCardViewModel vm);
        Task<ExperienceModel> GetExperienceProfile();
        void ChangeId(string newId);
    }
}
