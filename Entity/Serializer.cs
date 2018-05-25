using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DataChain.DataLayer;

namespace DataChain.EntityFramework
{
   public static class Serializer
    {
       

        public static SignatureEvidence DecodeSignature(byte[] sign) => throw new NotImplementedException();

        public static Transaction TransactionDecode(HexString rawTransaction)
        {
            // var byteData = rawTransaction.ToByteArray();
            throw new NotImplementedException();

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
       
        public static Account DeserializeAccount(AccountModel model)
        {
            return new Account();
        }

        public static Transaction DeserializeTransaction(TransactionModel model)
        {
            return new Transaction(
                model.Timestamp,
                RecordsMapping((IList<RecordModel>)model.Records),
                new HexString(model.TransactionHash), 
                new HexString(model.Signature),
                new HexString(model.PubKey));
        }

        public static TransactionModel SerializeTransaction(Transaction transaction)
        {
            var list = transaction.Data.Select(s => s.Value.ToString()).ToList();

            var result = ConcatenateData(list);
            return new TransactionModel()
            {
                PubKey = transaction.PubKey.ToByteArray(),
                Signature = transaction.Sign.ToByteArray(),
                RawData = result.ToHexString(),
                TransactionHash = transaction.Hash.ToByteArray(),
                Timestamp = transaction.TimeStamp

            };
        }

        public static Record DeserializeRecord(RecordModel model)
        {
            return new Record(model.Id, model.Name, new HexString(model.Value),model.Type)
            {
                Version = model.Id,
                Value = new HexString(model.Value),
                Name = model.Name
            };
        }

        public static BlockModel SerializeBlock(Block rawBlock)
        {
            List<TransactionModel> tx_list = new List<TransactionModel>();

            foreach(var tx in rawBlock.Metadata.CurrentTransactions)
             tx_list.Add(SerializeTransaction(tx));

            return new BlockModel()
            {
                BlockHash = rawBlock.Hash.ToByteArray(),
                PreviousHash = rawBlock.PreviousHash.ToByteArray(),
                Timestamp = rawBlock.TimeStamp,
                MerkleRoot = rawBlock.MerkleRoot.ToByteArray(),
                Transactions = tx_list
            };
        }

        public static RecordModel SerializeRecord(Record record)
        {
            return new RecordModel()
            {
                Name = record.Name,
                Value = record.Value?.ToByteArray(),
                Type = record.TypeRecord
            };
        }


        public static Block DeserializeBlock(BlockModel model)
        {
            return new Block(new HexString(model.BlockHash), 
                             new HexString(model.PreviousHash),
                             model.Timestamp,
                             model.Id,
                             new HexString(model.MerkleRoot),
                             ComputeMetadata()
                             );
        }

        public static string ConcatenateData(IList<string> list_data)
        {
            string result = null;

            for (int i= 0; i< list_data.Count(); i++)
            {

                result += list_data[i];
            }

            return result;
        }



        public static byte[] ToBinaryArray(string str) => UTF8Encoding.UTF8.GetBytes(str);

        public static BlockMetadata ComputeMetadata()
        {
            return new BlockMetadata();
        }

        
        private static IEnumerable<Transaction> TransactionsMapping(IList<TransactionModel> model)
        {
            return model.SelectMany(tx =>
            {
                IList<Transaction> rec = new List<Transaction>();

                for (int i = 0; i <= model.Count(); i++)
                {
                    rec.Add(new Transaction()
                    {
                        Instance = tx.Id,
                        Data = RecordsMapping(tx.Records.ToList()),
                        Hash = new HexString(tx.TransactionHash),
                        Sign = new HexString(tx.Signature),
                        TimeStamp = tx.Timestamp,
                    });
                }
                return rec;
            });
        }
        private static IEnumerable<Record> RecordsMapping(IList<RecordModel> model)
        {

            return model.SelectMany(record =>
            {
                IList<Record> rec = new List<Record>();

                for (int i = 0; i <= model.Count(); i++)
                {

                    rec.Add(new Record(model[i].Id, model[i].Name, new HexString(model[i].Value), model[i].Type));                  

                }
                return rec;
            });
        }
    }
}
