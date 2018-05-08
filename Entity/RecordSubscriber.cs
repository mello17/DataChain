using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;

namespace DataChain.EntityFramework
{
    public class RecordSubscriber : IRecordSubscriber
    {
        private DatachainContext db = new DatachainContext();

        public IReadOnlyList<Record> GetRecords()
        {
            var recordModel_list = db.Records.ToList();
            List<Record> records_list = new List<Record>();
            foreach(var record in recordModel_list)
            {
                records_list.Add(Serializer.DeserializeRecord(record));
            }

            return records_list.AsReadOnly();
        }

        public Task<Record> GetRecordsByValueAsync(HexString key)
        {
            throw new NotImplementedException();
        }

    }
}
