using System.Threading.Tasks;

namespace RobbieSenses.Interfaces
{
    public delegate void SenseEventHandler(string sense, string message);       

    public interface IBrain
    {                
        event SenseEventHandler SenseEvent;
        void WakeUp();
        void Hibernate();
        Task Say(string text);
        void Dispose();
    }
}
