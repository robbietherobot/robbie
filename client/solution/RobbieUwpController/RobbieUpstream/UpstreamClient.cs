using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace RobbieUpstream
{
    /// <summary>
    /// Upstream client to send log data to the demo server application.
    /// </summary>
    public class UpstreamClient
    {
        // todo: make this value configurable
        private const string UpstreamUri = "http://vision.robbie.net/api/";

        /// <summary>
        /// The base URL address of the Sitecore server to connect to.
        /// </summary>
        private readonly Uri baseAddress;

        /// <summary>
        /// The unique ID of the current upstream session;
        /// </summary>
        private readonly Guid sessionId;

        /// <summary>
        /// HTTP client used for the upstream connection.
        /// </summary>
        private readonly HttpClient client;

        /// <summary>
        /// Constructs a new upstream client object with a unique session ID.
        /// </summary>
        public UpstreamClient()
        {
            baseAddress = new Uri(UpstreamUri);
            sessionId = Guid.NewGuid();

            client = new HttpClient();
        }

        /// <summary>
        /// Sends event data to the server.
        /// </summary>
        /// <param name="senseEvent">The sense event to log.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
        public async Task RegisterEvent(SenseEvent senseEvent)
        {
            senseEvent.TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            senseEvent.SessionId = sessionId.ToString();

            await PostData(senseEvent, "event");
            
        }

        /// <summary>
        /// Posts data to the relative URL formatted as JSON.
        /// </summary>
        /// <param name="data">The object to post to the server.</param>
        /// <param name="relativeUrl">The relative URL to post to, indicating the required action.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
        private async Task PostData(object data, string relativeUrl)
        {
            var stringifiedData = JsonConvert.SerializeObject(data);
            await Post(stringifiedData, relativeUrl);
        }

        /// <summary>
        /// Posts a string to a relative URL.
        /// </summary>
        /// <param name="data">The string (data) to post.</param>
        /// <param name="relativeUrl">The relative URL to post to, indicating the required action.</param>
        /// <returns>A Task object for this method is asynchronous.</returns>
        private async Task Post(string data, string relativeUrl)
        {
            var absoluteUrl = new Uri(baseAddress, relativeUrl);
            var content = new HttpStringContent(data, UnicodeEncoding.Utf8, "application/json");
            await client.PostAsync(absoluteUrl, content);
        }
    }
}
