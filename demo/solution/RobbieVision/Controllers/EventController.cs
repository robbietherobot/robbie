using System.Web.Http;
using RobbieVision.Models;
using RobbieVision.IO;

namespace RobbieVision.Controllers
{
    /// <summary>
    /// Event controller handling the registration of newly logged events for a specific session.
    /// </summary>
    public class EventController : ApiController
    {
        public IHttpActionResult GetResult()
        {
            return Ok();
        }

        /// <summary>
        /// Handles posting a SenseEvent object, adding it to the local JSON event storage for the corresponding session.
        /// </summary>
        /// <param name="senseEvent">The SenseEvent object to store.</param>
        /// <returns>An IHttpActionResult object.</returns>
        public IHttpActionResult PostEvent(SenseEvent senseEvent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // save the latest frame capture to disk for the current session ID
            Imaging.SaveImage(senseEvent.SessionId, senseEvent.Capture);

            // remove the frame capture data from the event object
            senseEvent.Capture = null;

            // save the rest of the event data
            EventStorage.Add(senseEvent);

            return Ok();
        }
    }
}
