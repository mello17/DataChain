using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.DataLayer.Interfaces
{
   public  interface IRecordSubscriber
    {
        Task<IReadOnlyList<Record>> GetRecordsAsync();
        Task<Record> GetRecordsByValueAsync(HexString key);
    }
}
