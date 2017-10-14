using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace RobbieSpinalCord
{
    /// <summary>
    /// Handles a client server Sitecore connection.
    /// Stores cookies and contains logic to connect, post and retrieve data.   
    /// </summary>
    public class SitecoreConnection
    {
        // todo: make these values configurable
        private const string SitecoreServerUri = "http://www.robbie.net/";
        private const string RobbieUserAgent = @"Robbie/1.0 (Windows 10 IoT Core; Raspberry Pi 3 Model B)";

        /// <summary>
        /// The name of the Session ID cookie.
        /// </summary>
        private const string SessionIdCookieName = "ASP.NET_SessionId";

        /// <summary>
        /// The name of the Sitecore Analytics cookie.
        /// </summary>
        private const string ScAnalyticsCookieName = "SC_ANALYTICS_GLOBAL_COOKIE";

        /// <summary>
        /// The name of the authenticated cookie. 
        /// </summary>
        private const string AuthenticatedCookieName = "";

        /// <summary>
        /// Session ID cookie.
        /// </summary>
        /// <remarks>Cookies need to be stored in the connection, as the filter.CookieManager is a singleton and persists all cookies across separate HttpClients.</remarks>
        private HttpCookie sessionIdCookie;

        /// <summary>
        /// Analytics cookie.
        /// </summary>
        /// <remarks>Cookies need to be stored in the connection, as the filter.CookieManager is a singleton and persists all cookies across separate HttpClients.</remarks>
        private HttpCookie analyticsCookie;

        /// <summary>
        /// Authentication cookie.
        /// </summary>
        /// <remarks>Cookies need to be stored in the connection, as the filter.CookieManager is a singleton and persists all cookies across separate HttpClients.</remarks>
        private HttpCookie authenticationCookie;

        /// <summary>
        /// HttpBaseProtocol Filter
        /// </summary>
        private readonly HttpBaseProtocolFilter filter;

        /// <summary>
        /// HTTP client used for the Sitecore connection.
        /// </summary>
        private readonly HttpClient client;

        /// <summary>
        /// The base URL address of the Sitecore server to connect to.
        /// </summary>
        private readonly Uri baseAddress;        

        /// <summary>
        /// Constructs a new Sitecore Connection object.
        /// </summary>
        public SitecoreConnection()
        {                     
            baseAddress = new Uri(SitecoreServerUri);
            filter = new HttpBaseProtocolFilter();
            client = new HttpClient(filter);
            filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
            var headers = client.DefaultRequestHeaders;
            headers.UserAgent.TryParseAdd(RobbieUserAgent);

            ClearAllCookies();
        }

        /// <summary>
        /// Clears all cookies, used for initializing a new connection.
        /// </summary>
        private void ClearAllCookies()
        {
            var cookies = filter.CookieManager.GetCookies(baseAddress);
            foreach (var cookie in cookies)
            {
                DeleteCookie(cookie);
            }
        }

        /// <summary>
        /// Deletes a specific HttpCookie from the current filter.
        /// </summary>
        /// <param name="cookie">The HttpCookie oject to delete.</param>
        private void DeleteCookie(HttpCookie cookie)
        {
            filter.CookieManager.DeleteCookie(cookie);
        }

        /// <summary>
        /// Deletes a specific cookie from the current client by name.
        /// </summary>
        /// <param name="name">The name of the cookie to delete.</param>
        private void DeleteCookie(string name)
        {
            var cookies = filter.CookieManager.GetCookies(baseAddress);
            foreach (var cookie in cookies)
            {
                if(cookie.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    DeleteCookie(cookie);
                    return;
                }
            }
        }

        /// <summary>
        /// Sets the applicable connection HttpCookies for the current filter.
        /// </summary>
        private void SetCookies()
        {
            SetCookie(sessionIdCookie, SessionIdCookieName);
            SetCookie(analyticsCookie, ScAnalyticsCookieName);
            SetCookie(authenticationCookie, AuthenticatedCookieName);
        }

        /// <summary>
        /// Sets a cookie for the current connection.
        /// </summary>
        /// <param name="connectionCookie">The cookie object of the connection to set.</param>
        /// <param name="cookieName">The name of the cookie to set.</param>
        private void SetCookie(HttpCookie connectionCookie, string cookieName)
        {
            // if connection cookie settings are non-existant, delete the cookie in the cookie manager
            if (connectionCookie == null)
            {
                DeleteCookie(cookieName);
            }
            // if connection cookie settings are available, replace the current cookies with the ones in the connection
            else
            {
                var cookies = filter.CookieManager.GetCookies(baseAddress);
                foreach (var connectionManagerCookie in cookies)
                {
                    // find if cookie exists, if so, replace it
                    if(connectionCookie.Name.Equals(connectionManagerCookie.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        filter.CookieManager.DeleteCookie(connectionManagerCookie);
                        filter.CookieManager.SetCookie(connectionCookie);

                        // if cookie has been replaced, return
                        return;
                    }                    
                }

                // if no match has been found, set the cookie
                filter.CookieManager.SetCookie(connectionCookie);
            }
        }

        /// <summary>
        /// Stores all relevant cookies as connection specific parameters, setting the cookies from the properties to the filter.
        /// The Sitecore Connection properties are always leading.
        /// </summary>
        private void StoreCookies()
        {
            StoreSessionCookie();
            StoreAnalyticsCookie();
            StoreAuthenticationCookie();
        }

        /// <summary>
        /// Stores the Session ID cookie as a private property.
        /// </summary>
        private void StoreSessionCookie()
        {
            var cookie = GetCookie(SessionIdCookieName);
            if (cookie != null)
            {
                sessionIdCookie = cookie;
            }
        }

        /// <summary>
        /// Stores the Sitecore Analytics cookie as a private property.
        /// </summary>
        private void StoreAnalyticsCookie()
        {
            var cookie = GetCookie(ScAnalyticsCookieName);
            if (cookie != null)
            {
                analyticsCookie = cookie;
            }
        }

        /// <summary>
        /// stores authentication cookie as private property
        /// </summary>
        private void StoreAuthenticationCookie()
        {
            var cookie = GetCookie(AuthenticatedCookieName);
            if (cookie != null)
            {
                authenticationCookie = cookie;
            }
        }

        /// <summary>
        /// Gets a cookie from the filter.
        /// </summary>
        /// <param name="name">The name of the cookie to retrieve.</param>
        /// <returns>An HttpCookie object.</returns>
        private HttpCookie GetCookie(string name)
        {
            var cookies = filter.CookieManager.GetCookies(baseAddress);
            foreach (var cookie in cookies)
            {
                if(cookie.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return cookie;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the reponse data for a certain action, currently only used for intents, but extensible.
        /// </summary>
        /// <typeparam name="T">Can be any action that Robbie supports. For now: only intents.</typeparam>
        /// <param name="data">The action to get the response data for.</param>
        /// <returns>A deserialized JSON object, parsed from the raw response body.</returns>
        public async Task<T> GetData<T>(string data)
        {
            var reply = await GetData(data);
            reply = WebUtility.HtmlDecode(reply);
            var deserializedObject = JsonConvert.DeserializeObject<T>(reply);
            return deserializedObject;
        }

        /// <summary>
        /// Gets the reponse data from the request for the given intent.
        /// </summary>
        /// <param name="intent">The intent to get the response for.</param>
        /// <returns>The body data represented as a string, containing the response on the given intent.</returns>
        private async Task<string> GetData(string intent)
        {
            var url = new Uri(baseAddress, intent);                        

            return await GetBodyFromRequest(url);
        }        

        /// <summary>
        /// Gets the body data represented as a string from the given URL.
        /// </summary>
        /// <param name="url">The URL to get the body of.</param>
        /// <returns></returns>
        private async Task<string> GetBodyFromRequest(Uri url)
        {
            // set the cookies from the properties to the filter, the truth is always in the Sitecore Connection properties
            SetCookies();

            var reply = await client.GetAsync(url);
            // todo: you should not throw an exception if you do not handle it properly, otherwise, Robbie would crash completely - better return an empty response I reckon
            reply.EnsureSuccessStatusCode();
            var result = await reply.Content.ReadAsStringAsync();

            // always store the most important cookies to the connection
            StoreCookies();

            return result;
        }

        /// <summary>
        /// Posts data to the relative URL and converts the reply back to the expected object.
        /// </summary>
        /// <typeparam name="T">Can be any action that Robbie supports. For now: only intents.</typeparam>
        /// <param name="data">The action object to post.</param>
        /// <param name="relativeUrl">The relative URL to post the data to.</param>
        /// <returns>A deserialized JSON object, parsed from the raw response body.</returns>
        public async Task<T> PostData<T>(object data, string relativeUrl)
        {
            var stringifiedData = JsonConvert.SerializeObject(data);
            var request = await Post(stringifiedData, relativeUrl);
            request = WebUtility.HtmlDecode(request);
            var deserializedObject = JsonConvert.DeserializeObject<T>(request);
            return deserializedObject;
        }

        /// <summary>
        /// Posts data to the relative URL and returns the raw response formatted as a string.
        /// </summary>
        /// <param name="data">The action object to post.</param>
        /// <param name="relativeUrl">The relative URL to post the data to.</param>
        /// <returns>The raw response formatted as a string.</returns>
        private async Task<string> Post(string data, string relativeUrl)
        {
            // set the cookies from the properties to the filter, the truth is always in the Sitecore Connection properties
            SetCookies();

            var absoluteUrl = new Uri(baseAddress, relativeUrl);
            var content = new HttpStringContent(data, UnicodeEncoding.Utf8, "application/json");
            var reply = await client.PostAsync(absoluteUrl, content);
            var result = await reply.Content.ReadAsStringAsync();

            // always store the most important cookies to the connection
            StoreCookies();

            return result;
        }
    }
}
