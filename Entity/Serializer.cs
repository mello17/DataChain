using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using DataChain.Abstractions;

namespace DataChain.DataProvider
{
   public static class Serializer
    {

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
           return new Account()
            {
                Key =new AccountKey(new HexString(model.Key)),
                Login = model.Login,
                Password = new HexString(model.Password),
                Role = model.Role,
            };

        }

        public static Transaction DeserializeTransaction(TransactionModel model)
        {
            return new Transaction
            {
                Instance = model.Id,
                TimeStamp = model.Timestamp,
                Data = RecordsMapping((IList<RecordModel>)model.Records),
                Hash = new HexString(model.TransactionHash),
                Sign = new HexString(model.Signature),
                PubKey = new HexString(model.PubKey)
            };
        }
      
        

        public static TransactionModel SerializeTransaction(Transaction transaction)
        {
            var list = new List<string>();
            list = transaction.Data.Select(s => s.Value.ToString()).ToList();

            if(list.Count == 0)
            {
                list.Add("");
            }

            var result = ConcatenateData(list);
            return new TransactionModel()
            {
                Id = transaction.Instance,
                PubKey = transaction.PubKey.ToByteArray(),
                Signature = transaction.Sign.ToByteArray(),
                RawData = result.ToHexString(),
                TransactionHash = transaction.Hash.ToByteArray(),
                Timestamp = transaction.TimeStamp

            };
        }

        public static Record DeserializeRecord(RecordModel model)
        {
            return new Record(model.Id, model.Name, new HexString(model.Value), model.Type);
           
        }

        public static AccountModel SerializeAccount(Account account)
        {
            return new AccountModel()
            {
                Key = account.Key.Key.ToByteArray(),
                Login = account.Login,
                Password = account.Password.ToByteArray(),
                Role = account.Role
            };
        }

        public static BlockModel SerializeBlock(Block rawBlock)
        {
            List<TransactionModel> tx_list = new List<TransactionModel>();
            

            return new BlockModel()
            {
                BlockHash = rawBlock.Hash.ToByteArray(),
                PreviousHash = rawBlock.PreviousHash.ToByteArray(),
                Timestamp = rawBlock.TimeStamp,
                MerkleRoot = rawBlock.MerkleRoot.ToByteArray(),
            //    Transactions = tx_list
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
                             TransactionsMapping(model.Transactions, model.Id).ToList()
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

        public static BlockMetadata ComputeMetadata(IEnumerable<Transaction> tx)
        {
            var count = tx.Count();
            return new BlockMetadata()
            {
                CurrentTransactions = tx.ToList(),
                Instance = 1,
                TransactionCount = count
            };
        }


        public static IEnumerable<Transaction> TransactionsMapping(IEnumerable<TransactionModel> model, int id)
        {
            
            return model.SelectMany(tx =>
            {
                IList<Transaction> rec = new List<Transaction>();

                
               
                    if (id == tx.BlockModelId)
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
