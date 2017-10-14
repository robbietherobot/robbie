using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Analytics;
using Sitecore.Analytics.Tracking;

namespace Robbie.xDB
{
    public class ProfileHelper
    {
        // extra resource / info, not yet implemented: http://sitecorejourney.nileshthakkar.in/2015/10/how-to-set-personapattern-card.html

        public void ScoreProfile(Contact contact, string profileName, Dictionary<string, float> profileKeyValues)
        {
            var visit = Tracker.Current.Interaction;

            if (!visit.Profiles.ContainsProfile(profileName)) return;

            var profile = visit.Profiles[profileName];
            profile.Score(profileKeyValues);
        }

        public Dictionary<string, float> GetProfileValues(Contact contant, string profileName, List<string> keys)
        {
            var visit = Tracker.Current.Interaction;

            if (!visit.Profiles.ContainsProfile(profileName))
            {
                return new Dictionary<string, float>();
            }

            var profile = visit.Profiles[profileName];
            var profileKeyValues = new Dictionary<string, float>();
            foreach (var key in keys)
            {
                profileKeyValues.Add(key, profile[key]);
            }
            return profileKeyValues;
        }
    }
}
