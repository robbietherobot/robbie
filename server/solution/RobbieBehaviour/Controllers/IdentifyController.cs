using Newtonsoft.Json;
using RobbieBehaviour.Attributes;
using RobbieBehaviour.Models;
using Sitecore.Feature.Demo.Services;
using Sitecore.Foundation.Accounts.Providers;
using Sitecore.Foundation.SitecoreExtensions.Attributes;
using Sitecore.Foundation.SitecoreExtensions.Services;
using Sitecore.Services.Infrastructure.Web.Http;
using System;
using System.Globalization;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;

namespace RobbieBehaviour.Controllers
{
    /// <summary>
    /// Identification handling
    /// </summary>
    [SkipWebAPIAnalyticsTracking]
    public class IdentifyController : ServicesApiController
    {        
        /// <summary>
        /// contact profile provider
        /// </summary>
        private readonly IContactProfileProvider contactProfileProvider;

        /// <summary>
        /// profile provider
        /// todo: not used?
        /// </summary>
        private readonly IProfileProvider profileProvider;

        public IdentifyController(): this(new ContactProfileProvider(), new ProfileProvider())
        {            
        }

        public IdentifyController(IContactProfileProvider contactProfileProvider, IProfileProvider profileProvider)
        {
            this.contactProfileProvider = contactProfileProvider;
            this.profileProvider = profileProvider;
        }

        /// <summary>
        /// gets current identity. Only to test if controller is working
        /// </summary>
        /// <returns>test: get</returns>
        // GET: Identity
        [HttpGet]
        public IHttpActionResult Index()
        {
            var response = new Response
            {
                Test = "get "
            };
            return new JsonResult<Response>(response, new JsonSerializerSettings(), Encoding.UTF8, this);
        }

        /// <summary>
        /// Identifies person. vm only contains personID
        /// </summary>
        /// <param name="vm"></param>
        /// <returns>Profile of identified person</returns>
        [HttpPost]
        public IHttpActionResult Index([FromBody]IdentifyViewModel vm)
        {
            ITrackerService service = new TrackerService();
            service.IdentifyContact(vm.PersonId);

            var response = new ProfileViewModel(contactProfileProvider);

            var name = contactProfileProvider.PersonalInfo.FirstName;

            Guid guid;
            // if name is empty, unkown or it has a guid, treat it as "no name". The client should trigger the update name action
            if (string.IsNullOrEmpty(name) ||
                string.Compare(name, "unknown", true, CultureInfo.CurrentCulture) == 0 ||
                Guid.TryParse(name, out guid))
            {
                response.Name = string.Empty;
            }
            else
            {
                response.Name = name;
            }

            return new JsonResult<ProfileViewModel>(response, new JsonSerializerSettings(), Encoding.UTF8, this);
        }

        /// <summary>
        /// Logs a user in
        /// </summary>
        /// <param name="viewModel">personID and voiceID</param>
        /// <returns>Confirmation that the user has been authenticated</returns>
        [HttpPost]
        public IHttpActionResult Login([FromBody]LoginViewModel viewModel)
        {
            var response = new Response
            {
                Test = $"{viewModel.PersonId} logged in with password {viewModel.VoiceId}"
            };
            return new JsonResult<Response>(response, new JsonSerializerSettings(), Encoding.UTF8, this);
        }                
    }
}