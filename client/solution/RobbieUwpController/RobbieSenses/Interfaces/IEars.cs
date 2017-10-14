using System.Threading.Tasks;
using Windows.Storage.Streams;
using RobbieSenses.Input;

namespace RobbieSenses.Interfaces
{
    public delegate void UtteranceRecognizedHandler(string text);
    public delegate void EarsStateChangedHandler(EarsState state);

    public interface IEars
    {
        event UtteranceRecognizedHandler SpeechRecognized;
        event EarsStateChangedHandler EarsStateChanged;
        
        EarsState State { get; }
        void StartListening();
        void StopListening();
        Task<IRandomAccessStream> GetAudio(int duration = 3000);
    }
}
