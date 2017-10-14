using System.Threading.Tasks;

namespace RobbieSenses.Interfaces
{
    public delegate void FinishedPlayback();
    
    public interface IVoice
    {
        event FinishedPlayback FinishedPlaybackEventHandler;
        Task Say(string text);
    }
}
