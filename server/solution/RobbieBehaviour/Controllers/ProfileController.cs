using Newtonsoft.Json;
using RobbieBehaviour.Attributes;
using RobbieBehaviour.Models;
using Sitecore.Analytics;
using Sitecore.Feature.Accounts.Models;
using Sitecore.Feature.Accounts.Services;
using Sitecore.Feature.Demo.Models;
using Sitecore.Feature.Demo.Services;
using Sitecore.Foundation.Accounts.Providers;
using Sitecore.Foundation.SitecoreExtensions.Attributes;
using Sitecore.Foundation.SitecoreExtensions.Services;
using Sitecore.Services.Infrastructure.Web.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;

namespace RobbieBehaviour.Controllers
{
    [SkipWebAPIAnalyticsTracking]
    public class ProfileController : ServicesApiController
    {
        /// <summary>
        /// contact profile provider for handling xdb actions
        /// </summary>
        private readonly IContactProfileProvider contactProfileProvider;

        /// <summary>
        /// profile provider for handling profile and pattern cards
        /// </summary>
        private readonly IProfileProvider profileProvider;

        /// <summary>
        /// public constructor
        /// </summary>
        public ProfileController():this(new ProfileProvider(), new ContactProfileProvider())
        { }

        /// <summary>
        /// public constructor
        /// </summary>
        /// <param name="profileProvider">profile provider for profile and pattern cards</param>
        /// <param name="contactProfileProvider">profile provider for contacts</param>
        public ProfileController(IProfileProvider profileProvider, IContactProfileProvider contactProfileProvider)
        {
            this.profileProvider = profileProvider;
            this.contactProfileProvider = contactProfileProvider;
        }

        /// <summary>
        /// Gets current profile
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult Index()
        {
            var vm = new ProfileViewModel(contactProfileProvider);            
            return new JsonResult<ProfileViewModel>(vm, new JsonSerializerSettings(), Encoding.UTF8, this);
        }

        /// <summary>
        /// Updates profile
        /// </summary>
        /// <param name="vm"></param>
        /// <returns>confirmation on update</returns>
        [HttpPost]
        public IHttpActionResult UpdateProfile([FromBody]UpdateProfileViewModel vm)
        {
            // fills profile
            EditProfile profile = new EditProfile
            {
                FirstName = vm.FirstName,
                Gender = vm.Gender,                                
            };

            // if profile name is provided and this name is not know yet, set new profile is true
            var newProfile = false;
            if (!String.IsNullOrEmpty(vm.FirstName) && String.IsNullOrEmpty(contactProfileProvider.PersonalInfo.FirstName))
            {                                
                    newProfile = true;                                 
            }
            
            // update profile
            IContactProfileService service = new ContactProfileService();
            service.SetProfile(profile);
            
            // if new profile and profile has been set, set new outcome for the experience profile timeline
            if (newProfile && !String.IsNullOrEmpty(contactProfileProvider.PersonalInfo.FirstName))
            {
                var accountTrackerService = new AccountTrackerService(new AccountsSettingsService(), new TrackerService());
                accountTrackerService.TrackRegistration();    
                contactProfileProvider.Flush();            
            }

            //todo: change to something meaningful. Important, not urgent
            Response response = new Response();
            response.Test = $"profile updated";
            return new JsonResult<Response>(response, new JsonSerializerSettings(), Encoding.UTF8, this);
        }        

        /// <summary>
        /// Gets all experience data for current user
        /// </summary>
        /// <returns>experience data</returns>
        [HttpGet]
        public IHttpActionResult Experience()
        {
            var data = GetData();
            return new JsonResult<ExperienceData>(data, new JsonSerializerSettings(), Encoding.UTF8, this);
        }

        /// <summary>
        /// posts emotion scores. Will be update in the users profile
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult UpdateEmotion([FromBody]ProfileCardViewModel viewModel)
        {
            var emotionScores = Tracker.Current.Interaction.Profiles["Emotion"];

            var scores = new Dictionary<string, float>
            {
                {"Anger", viewModel.Anger},
                {"Contempt", viewModel.Contempt},
                {"Disgust", viewModel.Disgust},
                {"Fear", viewModel.Fear},
                {"Happiness", viewModel.Happiness},
                {"Neutral", viewModel.Neutral},
                {"Sadness", viewModel.Sadness},
                {"Surprise", viewModel.Surprise}
            };

            emotionScores.Score(scores);            

            contactProfileProvider.Flush();
            var data = GetData();
            
            return new JsonResult<ExperienceData>(data, new JsonSerializerSettings(), Encoding.UTF8, this);
        }

        /// <summary>
        /// gets experience data as model
        /// </summary>
        /// <returns>experience data</returns>
        private ExperienceData GetData()
        {
            return new ExperienceData(contactProfileProvider, profileProvider);
        }
    }
}