using System;
using System.Threading.Tasks;

namespace RobbieSenses.Interfaces
{
    public delegate void PersonFound();
    public interface IEyes
    {
        void WakeUp();
        void Hibernate();
        void ShowEmotion(string emotion);
        void Dispose();
        Task<Guid> CreatePerson();
        Task<string> UpdatePerson(Guid personId, string name);
        Task StoreFaceFor(string name);
    }
}
