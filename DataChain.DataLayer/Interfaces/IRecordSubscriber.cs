using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.DataLayer.Interfaces
{
   public  interface IRecordSubscriber
    {
        IReadOnlyList<Record> GetRecords();
        Task<Record> GetRecordsByNameAsync(string name);
    }
}
