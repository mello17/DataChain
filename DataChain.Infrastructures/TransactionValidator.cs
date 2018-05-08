using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using DataChain.EntityFramework;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;


namespace DataChain.Infrastructures
{
   public class TransactionValidator
    {
        private IBlockSubscriber blcSubscribe;
        private ITransactionSubscriber txSubscribe;

        public TransactionValidator(IBlockSubscriber _blcSubscribe)
        {
            blcSubscribe = _blcSubscribe;
        }

        public async Task<Transaction> ValidateTransaction( JObject jsonTransaction, string key)
        {
            var records = (JArray)jsonTransaction["records"];
            IEnumerable<Record> referenceRecords =  ValidateRecords(records);

            DateTime date = DateTime.UtcNow;

            var recordValue = records.Select(s=>(string)s["value"]).ToList();
            var concatenateData = Serializer.ConcatenateData(recordValue);
            var transactionHash = Serializer.ComputeHash(concatenateData.ToHexString());

            string secret = null;
            ECKeyValidator eckey = new ECKeyValidator();
            try
            {
                 secret = File.ReadAllText("~/DataChain/privKey/key.xml", UTF8Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {

            }
            catch (IOException)
            {

            }

            string sign = null;

            try
            {

                sign = eckey.SignData(concatenateData, secret);

                if (!eckey.VerifyMessage(concatenateData, sign, key))
                {
                    throw new InvalidTransactionException("");
                }
            }
            catch
            {
                throw new InvalidTransactionException("");
            }

            Signature auth = new Signature(HexString.Parse(sign), HexString.Parse(key));
            byte[] rawSign = SerializeSignature(auth);
            Transaction transaction = new Transaction(date, referenceRecords ,new HexString(transactionHash),new HexString( rawSign));


            BlockBuilder blockBulider = new BlockBuilder(blcSubscribe);
            List<Transaction> tx_list = new List<Transaction>() {transaction };

            tx_list.AddRange(await Task.WhenAll(referenceRecords.Select(async rec => {

                 ValidateRecords(records);
                Transaction tx = new Transaction(date, referenceRecords, new HexString(transactionHash), new HexString(rawSign));
                await blockBulider.GenerateBlock(new Block(), tx_list);
                return tx; 
                }
            )));

            await txSubscribe.AddTransactionAsync(tx_list);

            return transaction;



        }

        private  IList<Record> ValidateRecords(JArray records)
        {
            IList<Record> rec = new List<Record>();
            try
            {
                foreach (var recData in records)
                {
                    if (recData["version"] == null || !(recData["version"] is JToken) || (int)recData["version"] <= 0 )
                    {
                        throw new InvalidTransactionException("InvalidRecordVersion");
                    }

                    if (recData["value"] == null || !(recData["value"] is JToken))
                    {
                        throw new InvalidTransactionException("InvalidRecordValue");
                    }

                    if (recData["name"] == null ||  !(recData["name"] is JToken))
                    {
                        throw new InvalidTransactionException("InvalidRecordName");
                    }

                    rec.Add(new Record()
                    {
                        Version = (int)recData["version"],
                        Name = (string)recData["name"],
                        Value = new HexString( HexString.Parse((string)recData["value"]).ToByteArray())
                    });
                }
            }
            catch (InvalidCastException)
            {
                throw new InvalidTransactionException("");
            }

            return rec;
        }

        private static void ValidateAuthentication(IReadOnlyList<Signature> authentication, byte[] Hash)
        {
            foreach (Signature evidence in authentication)
            {
            //    if (!evidence.(Hash))
              //      throw new InvalidTransactionException("InvalidSignature");
            }
        }

        private byte[] SerializeSignature(Signature sign)
        {
            byte[] buffer = new byte[1024];
            using(MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryWriter reader = new BinaryWriter(stream);
                reader.Write(sign.PublicKey.ToByteArray());
                reader.Write(sign.SignatureData.ToByteArray());
                reader.Close();

            }
            return buffer;
        }
    }
}
