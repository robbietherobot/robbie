using Sitecore.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace RobbieBehaviour.Attributes
{
    /// <summary>
    /// cancels tracking of WebAPI request
    /// </summary>
    public class SkipWebAPIAnalyticsTracking: ActionFilterAttribute
    {
        /// <summary>
        /// cancels tracking of WebAPI action
        /// </summary>
        /// <param name="actionContext">actionContext</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (Tracker.IsActive)
            {
                Tracker.Current?.CurrentPage?.Cancel();
            }
        }
    }
}