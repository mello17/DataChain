using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DataChain.DataLayer;

namespace DataChain.EntityFramework
{
   public static class Serializer
    {
        public static string RecordDecode(byte[] data)
        {
            StringBuilder sBuilder = new StringBuilder();
            using (MD5 hasher = MD5.Create())
            {
               var md5hash = hasher.ComputeHash(Encoding.Default.GetBytes(data.ToString()));
                for (int i = 0; i < md5hash.Length; i++)
                {
                    
                    sBuilder.Append(md5hash[i].ToString("x2"));
                }

            }
            return sBuilder.ToString();

        }

        public static byte[] ToHexString(this string rawString)
        {
            byte[] bytes = new byte[rawString.Length];
            for (int i = 0; i < rawString.Length; i++)
                bytes[i] = Convert.ToByte(rawString[i]);
            return bytes;
        }

        public static byte[] ComputeHash(byte[] data)
        {
            byte[] hash;

            using(SHA256 hasher = SHA256.Create())
            {
               hash = hasher.ComputeHash(hasher.ComputeHash(data));
            }

            return hash;
        }

        public static Transaction DeserializeTransaction(TransactionModel model)
        {
            return new Transaction(model.Id,
                model.Timestamp,
                RecordsMapping((IList<RecordModel>)model.Records),
                new HexString(model.TransactionHash));
        }

        public static Block DeserializeBlock(BlockModel model)
        {
            return new Block(new HexString(model.BlockHash), 
                             new HexString(model.PreviousHash),
                             model.Timestamp,
                             model.Id,
                             ComputeMetadata()
                             );
        }

        public static BlockMetadata ComputeMetadata()
        {
            return new BlockMetadata();
        }

        //public static TransactionModel SerializeTransaction(Transaction tx)
        //{
        //    new TransactionModel()
        //    {
        //        RawData = tx.
        //    };
        //}

        private static IEnumerable<Record> RecordsMapping(IList<RecordModel> model)
        {

            return model.SelectMany(record =>
            {
                IList<Record> rec = new List<Record>();

                for (int i = 0; i <= model.Count(); i++)
                {
                    var recordName = model[i].Name;
                    var recordValue = model[i].Value;
                    var recordKey = model[i].
                    var recordId = model[i].Instance;

                    rec.Add(new Record()
                    {
                        Id = recordId,
                        Value = new HexString(recordValue),
                        Name = recordName,

                    });

                }
                return rec;
            });
        }
    }
}
