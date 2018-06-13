using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataChain.Abstractions;
using DataChain.Abstractions.Interfaces;

namespace DataChain.DataProvider
{
    public class RecordRepository : IRecordRepository
    {
        private DatachainContext db = new DatachainContext();

        public IReadOnlyList<Record> GetRecords(string recordName, TypeData type)
        {

           
            var recordModel_list = db.Records.Where(b => b.Name == recordName && b.Type == type);
           
            var records_list = new List<Record>();
            foreach(var record in recordModel_list)
            {
                records_list.Add(Serializer.DeserializeRecord(record));
            }

            return records_list.AsReadOnly();
        }

        
        public async Task<Record>  GetRecordByNameAsync(string name)
        {
           var records = db.Records.Where(b => b.Name == name);
           List<Record> records_list = new List<Record>();

           foreach(var record in records)
           {
                records_list.Add(Serializer.DeserializeRecord(record));
           }

            return await Task.FromResult(records_list.SingleOrDefault());
        }

        public async Task AddRecordsAsync(IEnumerable<Record> records)
        {

            var records_list = new List<RecordModel>();

            foreach (var record in records)
            {
                records_list.Add(Serializer.SerializeRecord(record));
            }

            db.Records.AddRange(records_list);
            await db.SaveChangesAsync();

        }

    }
}
