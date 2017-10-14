using System.Collections.Generic;
using System.Threading.Tasks;

namespace RobbieSenses.Interfaces
{
    public interface IIntent
    {
        Task<IList<IAction>> HandleIntent(); 
    }
}
