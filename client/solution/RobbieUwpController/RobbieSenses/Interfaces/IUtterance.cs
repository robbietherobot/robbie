using Microsoft.Cognitive.LUIS;
using System.Threading.Tasks;

namespace RobbieSenses.Interfaces
{
    public interface IUtterance
    {
        Task<LuisResult> GetIntent(string utterance);        
    }
}
