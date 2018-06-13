using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataChain.Abstractions.Interfaces
{
   public  interface IRecordRepository
    {
        IReadOnlyList<Record> GetRecords(string recordName, TypeData type);
        Task<Record> GetRecordByNameAsync(string name);
        Task AddRecordsAsync(IEnumerable<Record> records);
    }
}
