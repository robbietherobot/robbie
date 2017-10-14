using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using RobbieVision.Models;

namespace RobbieVision.Controllers
{
    /// <summary>
    /// Home controller handling the listing of available sessions.
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var viewModel = new SessionViewModel()
            {
                Sessions = GetSessionList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Gets a list of available session objects, derived from the data storage directory listing.
        /// </summary>
        /// <returns>A list of Session objects.</returns>
        private List<Session> GetSessionList()
        {
            const string extension = ".json";
            var sessions = new List<Session>();

            var directory = new DirectoryInfo(Server.MapPath("DataStorage/"));
            var files = directory.GetFiles($"*{extension}");

            foreach (var file in files.OrderByDescending(file => file.LastWriteTime))
            {
                sessions.Add(new Session()
                {
                    SessionId = file.Name.Replace(extension, string.Empty),
                    TimeStamp = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            return sessions;
        }
    }
}