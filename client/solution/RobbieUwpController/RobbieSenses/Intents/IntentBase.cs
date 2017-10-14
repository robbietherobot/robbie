using Microsoft.Cognitive.LUIS;
using System.Threading.Tasks;
using RobbieSenses.Interfaces;
using RobbieSpinalCord.Interfaces;
using System.Collections.Generic;

namespace RobbieSenses.Intents
{
    /// <summary>
    /// Base implementation of the intent interface to be used by all intent types.
    /// </summary>
    public abstract class IntentBase : IIntent
    {
        protected IList<IAction> Actions = new List<IAction>();
        protected readonly IClient Client;
        protected readonly LuisResult Intent;

        protected IntentBase(LuisResult intent)
        {
            Intent = intent;
        }

        protected IntentBase(LuisResult intent, IClient client) : this(intent)
        {
            Client = client;            
        }
        
        public abstract Task<IList<IAction>> HandleIntent();                
    }
}
