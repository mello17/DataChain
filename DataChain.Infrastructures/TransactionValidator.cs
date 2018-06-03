﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using DataChain.DataProvider;
using DataChain.Abstractions;
using DataChain.Abstractions.Interfaces;


namespace DataChain.Infrastructure
{
    public class TransactionValidator : ITransactionValidator
    {

        private ITransactionSubscriber txSubscribe;
        private IRecordSubscriber recordSubscriber;

        public TransactionValidator(ITransactionSubscriber _txSubscribe, IRecordSubscriber _recordSubscriber)
        {
            txSubscribe = _txSubscribe;
            recordSubscriber = _recordSubscriber;
        }
 

        public async Task<Transaction> ValidateTransaction( object records, string key)
        {

            IEnumerable<Record> referenceRecords = null;
            if (records is JArray)
            {
                 referenceRecords = ValidateRecords((JArray)records);
            }
            else if (records is IEnumerable<Record>)
            {
                referenceRecords = (IEnumerable<Record>)records;
            }
            try
            {
                await recordSubscriber.AddRecordsAsync(referenceRecords);
            }
            catch (Exception)
            {
                return null;
            }


            DateTime date = DateTime.UtcNow;

            var recordValue = referenceRecords.Select(s=> s.Value.ToString()).ToList();
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

            try
            {

                sign = eckey.SignData(concatenateData, secret);

                if (!eckey.VerifyMessage(concatenateData, sign, key))
                {
                    throw new InvalidTransactionException("Signature is not valid");
                }

            }

            catch(Exception ex)
            {
                throw new InvalidTransactionException(ex.Message);
            }

            SignatureEvidence auth = new SignatureEvidence(new HexString(sign.ToHexString()), new HexString(key.ToHexString()));
            byte[] rawSign = SerializeSignature(auth);
           
            BlockBuilder blockBulider = new BlockBuilder(new BlockSubscriber(), txSubscribe);
            List<Transaction> tx_list = new List<Transaction>();
            Transaction transaction = null;

            tx_list.AddRange(await Task.WhenAll(referenceRecords.Select(async rec => {
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

            return transaction;

        }

        private  IList<Record> ValidateRecords(JArray records)
        {
            IList<Record> rec = new List<Record>();
            try
            {
                foreach (var recData in records)
                {
                    if (string.IsNullOrEmpty((string)recData["Version"])  || !(recData["Version"] is JToken) || (int)recData["Version"] <= 0 )
                    {
                        throw new InvalidTransactionException("InvalidRecordVersion");
                    }

                    if (recData["Value"] == null || !(recData["Value"] is JToken))
                    {
                        throw new InvalidTransactionException("InvalidRecordValue");
                    }

                    if (string.IsNullOrEmpty((string)recData["Name"]) ||  !(recData["Name"] is JToken))
                    {
                        throw new InvalidTransactionException("InvalidRecordName");
                    }

                    if(string.IsNullOrEmpty((string)recData["TypeRecord"]) || !(recData["TypeRecord"] is JToken))
                    {
                        throw new InvalidTransactionException("InvalidTypeData");
                    }


                    TypeData enumTypeData ;
                    TryDataTypeParse((string)recData["TypeRecord"], out enumTypeData);

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
