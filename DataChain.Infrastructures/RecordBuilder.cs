using DataChain.Abstractions;
using DataChain.DataProvider;
using DataChain.Abstractions.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Runtime;
using System.Runtime.Serialization.Formatters.Binary;

namespace DataChain.Infrastructure
{
    public class RecordBuilder
    {
        private IRecordRepository recordSubscriber = new RecordRepository();
        private IAccountRepository accountSubscriber = new AccountRepository();

        public IList<Record> ValidateRecords(JArray records)
        {
            IList<Record> rec = new List<Record>();
            try
            {
                foreach (var recData in records)
                {
                    if (string.IsNullOrEmpty((string)recData["Version"]) || !(recData["Version"] is JToken) || (int)recData["Version"] <= 0)
                    {
                        throw new InvalidTransactionException("InvalidRecordVersion");
                    }

                    if (recData["Value"] == null || !(recData["Value"] is JToken))
                    {
                        throw new InvalidTransactionException("InvalidRecordValue");
                    }

                    if (string.IsNullOrEmpty((string)recData["Name"]) || !(recData["Name"] is JToken))
                    {
                        throw new InvalidTransactionException("InvalidRecordName");
                    }

                    if (string.IsNullOrEmpty((string)recData["TypeRecord"]) || !(recData["TypeRecord"] is JToken))
                    {
                        throw new InvalidTransactionException("InvalidTypeData");
                    }

                    TryDataTypeParse((string)recData["TypeRecord"], out TypeData enumTypeData);

                    rec.Add(new Record((int)recData["Version"],
                        (string)recData["Name"],
                        new HexString(recData["Value"]["Value"].Values().Select(b => byte.Parse(b.ToString())).ToArray()),
                        enumTypeData
                    ));
                }
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidTransactionException(ex.Message);
            }

            return rec;
        }

       public async Task PostRecordsAsync(IEnumerable<Record> records)
        {

            foreach(var record in records)
            {
               if(record.TypeRecord == TypeData.Account)
                {
                    using(MemoryStream stream = new MemoryStream(record.Value.ToByteArray()))
                    {
                        BinaryFormatter form = new BinaryFormatter();
                        Account account = (Account)form.Deserialize(stream);
                        accountSubscriber.AddUser(account);

                    }
                }
            }

            try
            {
                await recordSubscriber.AddRecordsAsync(records);
            }
            catch (Exception ex)
            {
                throw new InvalidTransactionException(ex.Message);
            }
        }

        public static void TryDataTypeParse(string typeData, out TypeData enumType)
        {
            switch (typeData)
            {
                case "0":
                    enumType = TypeData.Host;
                    break;
                case "1":
                    enumType = TypeData.Content;
                    break;
                case "2":
                    enumType = TypeData.Account;
                    break;
                default: throw new InvalidCastException();
            }
        }

    }
}
