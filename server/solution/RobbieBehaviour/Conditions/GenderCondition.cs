using System;
using Sitecore.Analytics;
using Sitecore.Diagnostics;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;
using Sitecore.Foundation.Accounts.Providers;

namespace RobbieBehaviour.Conditions
{
    /// <summary>
    /// Gender condition, used for personalization of content
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenderCondition<T> : StringOperatorCondition<T> where T : RuleContext
    {
        /// <summary>
        /// Contact profile provider
        /// </summary>
        private readonly IContactProfileProvider contactProfileProvider;

        /// <summary>
        /// public constructur
        /// </summary>
        public GenderCondition()
        {
            contactProfileProvider = new ContactProfileProvider();
        }

        /// <summary>
        /// Value to validate against
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// Executes rule
        /// </summary>
        /// <param name="ruleContext"></param>
        /// <returns></returns>
        protected override bool Execute(T ruleContext)
        {
            Assert.ArgumentNotNull(ruleContext, "ruleContext");

            if(Tracker.Current == null ||
                    !Tracker.Current.IsActive ||
                    Tracker.Current.Contact == null)
                return false;

            var result = false;

            // Get gender. If equals, return true
            var gender = contactProfileProvider.PersonalInfo.Gender;
            if (Value.Equals(gender, StringComparison.InvariantCultureIgnoreCase))
                result = true;

            return result;
        }
    }
}