using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Analytics;

namespace Robbie.xDB
{
    public class QueryStringParser
    {

        public QueryStringParser()
        {
            

        }

        // http://robbie/joke?identifier={0}&firstname={1}&gender={2}&age={3}&emotion-anger={4}&emotion-neutral={5}&emotion-happiness={6}

        private const string IdentifierKey = "identifier";
        private const string FirstNameKey = "firstname";
        private const string GenderKey = "gender";
        private const string AgeKey = "age";
        private const string EmotionProfileKey = "emotion";

        public void Process(NameValueCollection queryString)
        {
            var cf = new ContactFactory();
            var ph = new ProfileHelper();

            var contact = Tracker.Current?.Session?.Contact;

            if (!string.IsNullOrEmpty(queryString[IdentifierKey]))
            {
                cf.GetContact(queryString[IdentifierKey]);
            }


            var firstName = string.Empty;
            var gender = string.Empty;
            var age = 0d;

            if (!string.IsNullOrEmpty(queryString[FirstNameKey]))
            {
                firstName = queryString[FirstNameKey];
            }

            if (!string.IsNullOrEmpty(queryString[GenderKey]))
            {
                gender = queryString[GenderKey];
            }

            if (!string.IsNullOrEmpty(queryString[AgeKey]))
            {
                double.TryParse(queryString[AgeKey], out age);
            }


            if (contact != null)
            {
                cf.SetPersonalData(contact, firstName, gender, age);

                //cf.SetProfilePicture();

                var keyValues = new Dictionary<string, float>();
                foreach (var key in queryString.Keys)
                {
                    if(key.ToString().StartsWith($"{EmotionProfileKey}-"))
                    {
                        float x;
                        if (float.TryParse(queryString[key.ToString()], out x))
                        {
                            keyValues.Add(
                                key.ToString().Replace($"{EmotionProfileKey}-", string.Empty),
                                x);
                        }
                    }
                }

                if (keyValues.Count > 0)
                {
                    ph.ScoreProfile(contact, "emotion", keyValues);
                }

                cf.ReleaseAndSave(contact);
            }
        }
    }
}
