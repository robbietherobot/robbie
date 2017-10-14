using System;
using RobbieSpinalCord.Actions;
using RobbieSpinalCord.Models;
using System.Threading.Tasks;
using RobbieSpinalCord.Interfaces;

namespace RobbieSpinalCord
{
    /// <summary>
    /// Sitecore client implementation handling the different actions like Intents and Identify.
    /// Uses a SitecoreConnection object for Sitecore server connectivity.
    /// </summary>
    public class SitecoreClient : IClient
    {
        /// <summary>
        /// The unique person ID adapted from the Cognitive Services Face API to uniquely identify a person.
        /// </summary>
        public string PersonId { get; set; }
        
        /// <summary>
        /// The Sitecore Connection object to manage the Sitecore server connection.
        /// </summary>
        private readonly SitecoreConnection connection;

        /// <summary>
        /// Constructs a new empty Client object
        /// </summary>
        public SitecoreClient() 
        {
            connection = new SitecoreConnection();
        }

        /// <summary>
        /// Constructs a new Client object based on the given user ID.
        /// </summary>
        /// <param name="personId">The person ID of the Cognitive Services Face API.</param>
        public SitecoreClient(string personId) : this()
        {
            PersonId = personId;
        }        

        /// <summary>
        /// Gets the personalized action from the Sitecore server based on the given intent.
        /// </summary>
        /// <param name="intent">The intent to get the desired action for.</param>
        /// <returns>An IntentAction object describing the required action for this intent.</returns>
        public async Task<IntentAction> GetIntentAction(string intent)
        {
            var reply = await connection.GetData<IntentReply.Rootobject>(intent);

            return new IntentAction { Action = reply.response.action, Reply = reply.response.reply, Emotion = reply.response.emotion };
        }

        /// <summary>
        /// Gets the profile for the current user. If none exists, a new one will be created.
        /// </summary>
        /// <returns>The profile object for the current user.</returns>
        public async Task<IdentifyResponse> GetProfile()
        {
            await connection.GetData<IdentifyResponse>("api/profile");
            return null;
            // todo: implement call to sitecore
            
        }       

        /// <summary>
        /// Logs in into Sitecore using the API, requiring a (previously identified) person ID and a voice ID.
        /// </summary>
        /// <param name="voiceId">The voice ID to authenticate on.</param>
        /// <returns>A profile object of the logged in user.</returns>
        /// <remarks>Not yet implemented.</remarks>
        public Profile Login(string voiceId)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Sets the ID on the server for current connection, using the ID the object is constructed with.
        /// </summary>
        /// <returns>An IdentifyResponse object, containing the response on the identification process.</returns>  
        public async Task<IdentifyResponse> Identify()
        {
            var model = new IdentifyViewModel { PersonId = PersonId };
            return await connection.PostData<IdentifyResponse>(model, "api/Identify");
        }

        /// <summary>
        /// Update the profile info on the server.
        /// </summary>
        /// <param name="profile">The (new) profile of the current person to set.</param>
        /// <returns>The response of the server on the updating process, formatted as a string.</returns>
        public async Task<string> UpdateProfile(Profile profile)
        {
            var viewModel = new UpdateProfileViewModel(profile);
            var reply = await connection.PostData<Response>(viewModel, "api/Profile/UpdateProfile");
            return reply.Test;
        }

        /// <summary>
        /// Update the profile emotion data on the server.
        /// </summary>
        /// <param name="vm">The ProfileCardViewModel object containing the emotion profile data.</param>
        /// <returns>The updated experience model returned by the server.</returns>
        public async Task<ExperienceModel> UpdateProfileEmotions(ProfileCardViewModel vm)
        {
            var reply = await connection.PostData<ExperienceModel>(vm, "api/Profile/UpdateEmotion");
            return reply;
        }

        /// <summary>
        /// Returns the experience profile of the current person Robbie is interacting with.
        /// </summary>
        /// <returns>The current experience model as returned by the server.</returns>
        public async Task<ExperienceModel> GetExperienceProfile()
        {
            var reply = await connection.GetData<ExperienceModel>("api/Profile/Experience");
            return reply;
        }

        /// <summary>
        /// Sets a new person ID for this client.
        /// </summary>
        /// <param name="newPersonId">The new person ID to set for this client.</param>
        public void ChangeId(string newPersonId)
        {
            PersonId = newPersonId;
        }
    }
}
