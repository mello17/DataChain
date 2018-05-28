using System;
using System.Collections.Generic;


namespace DataChain.Abstractions.Interfaces
{
   public  interface IRecordSubscriber
    {
        IReadOnlyList<Record> GetRecords(string recordName, TypeData type);
        Record GetRecordByNameAsync(string name);
    }
}
