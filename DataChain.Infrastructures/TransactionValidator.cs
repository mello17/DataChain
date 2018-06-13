using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using DataChain.DataProvider;
using DataChain.Abstractions;
using DataChain.Abstractions.Interfaces;


namespace DataChain.Infrastructure
{
    public class TransactionValidator : ITransactionValidator
    {

        private readonly ITransactionRepository txSubscribe;
        private readonly IRecordRepository recordSubscriber;
        private readonly RecordBuilder recordBuilder = new RecordBuilder();

        public TransactionValidator()
        {
            txSubscribe = new TransactionRepository();
            recordSubscriber = new RecordRepository();
           // recordBuilder = _recordBuilder;
        }
 

        public async Task<Transaction> ValidateTransaction( object records, string key)
        {

            IEnumerable<Record> referenceRecords = null;
            if (records is JArray)
            {
                referenceRecords = recordBuilder.ValidateRecords((JArray)records);
            }
            else if (records is IEnumerable<Record>)
            {
                referenceRecords = (IEnumerable<Record>)records;
            }
            else if (records is Record)
            {
                referenceRecords = new[] { (Record)records };
            }
            else return null;

            DateTime date = DateTime.UtcNow;

            var recordValue = referenceRecords.Select(s => s.Value.ToString()).ToList();
            var concatenateData = Serializer.ConcatenateData(recordValue);
            var transactionHash = Serializer.ComputeHash(concatenateData.ToHexString());

            string secret = null;
            ECKeyValidator eckey = new ECKeyValidator();
            try
            {
                secret = File.ReadAllText("/DataChain/privKey/key.xml", UTF8Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }

            string sign = null;

            sign = eckey.SignData(concatenateData);

            if (!eckey.VerifyMessage(concatenateData, sign,key))
            {
                throw new InvalidTransactionException("Signature is not valid");
            }

            SignatureEvidence auth = new SignatureEvidence(new HexString(sign.ToHexString()), new HexString(key.ToHexString()));
            byte[] rawSign = SerializeSignature(auth);


            List<Transaction> tx_list = new List<Transaction>();
            Transaction transaction = null;

            tx_list.AddRange(await Task.WhenAll(referenceRecords.Select(async rec =>
            {
                await Task.Delay(100);
                transaction = new Transaction(date, referenceRecords, new HexString(transactionHash),
                    new HexString(sign.ToHexString()),
                    new HexString(key.ToHexString()));

                return transaction;
            }
            )));

            try
            {
               await txSubscribe.AddTransactionAsync(tx_list);
            }

            catch (InvalidTransactionException)
            {
                return null;
            }

            try
            {
                await recordBuilder.PostRecordsAsync(referenceRecords);
            }
            catch (Exception)
            {
                return null;
            }

            return transaction;

        }

        public byte[] SerializeSignature(SignatureEvidence sign)
        {
            byte[] buffer= new byte[1154];
            using(MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(sign.PublicKey.ToByteArray());
                writer.Write(sign.SignatureData.ToByteArray());
                writer.Close();

            }
            return buffer;
        }
    }
}
