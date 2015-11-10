using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Data.Repository
{
    public interface IRepository<T>
    {
        T Get(int id);

    }
}
