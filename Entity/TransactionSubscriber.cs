﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;

namespace DataChain.EntityFramework
{
   public class TransactionSubscriber : ITransactionSubscriber
    {
        private DatachainContext db = new DatachainContext();
        private IBlockSubscriber blcSubscribe = new BlockSubscriber();


        public  List<Transaction> GetLastTransactionAsync()
        {

            IEnumerable<TransactionModel> tx_list;
            try
            {
                tx_list = db.Transactions.Where(x => !(x.BlockModelId.HasValue) );
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            var raw_tx_list = new List<Transaction>();
            foreach(var tx in tx_list)
            {
                raw_tx_list.Add(Serializer.DeserializeTransaction(tx));
            }
            

            return raw_tx_list;
        }

        public async Task<byte[]> AddTransactionAsync(IEnumerable<Transaction> transactions)
        {
            IList<TransactionModel> tx_list = new List<TransactionModel>();
            string result = null;

            foreach (var tx in transactions)
            {
                if(tx.Data.Select(s=>s.Value).First() == null)
                {
                    continue;
                }

                var list = tx.Data.Select(s =>s.Value.ToString()).ToList();

                result = Serializer.ConcatenateData(list);

                var model = new TransactionModel()
                {
                  
                    TransactionHash = tx.Hash.ToByteArray(),
                    RawData = Serializer.ToBinaryArray( result ),
                    Timestamp = tx.TimeStamp,
                    Signature = tx.Sign.ToByteArray(),
                    PubKey = tx.PubKey.ToByteArray(),
                };

               var txs = db.Transactions.ToList();
               
               var conflict = txs.Where(b =>  b.RawData == model.RawData 
                                                       && b.PubKey == model.PubKey 
                                                       && b.Signature == model.Signature
                                                       && b.TransactionHash == model.TransactionHash
                                                       ).ToList();
                if(conflict.Count != 0)
                {
                    throw new InvalidTransactionException("Optimistic concurrency");
                }
                tx_list.Add(model);
            }


            try
            {

                db.Transactions.AddRange(tx_list);
                await db.SaveChangesAsync();
            }
            catch(DbEntityValidationException ex)
            {
                throw new InvalidOperationException(ex.EntityValidationErrors.First()
                    .ValidationErrors
                    .FirstOrDefault()
                    .ErrorMessage);
            }

            return Serializer.ComputeHash(Serializer.ToBinaryArray(result));
        }

        [global::System.Diagnostics.Contracts.ContractRuntimeIgnored]
        public async Task<Transaction> GetTransactionAsync(int index)
        {
            var tx = await db.Transactions.FindAsync(index);

            if (tx == null)
            {
                return null;
            }

            Transaction response = Serializer.DeserializeTransaction(tx);

            return response;

        }

        public async Task<Transaction> GetTransactionAsync(byte[] hash)
        {
            TransactionModel tx = null; 

            try
            {
                 tx = db.Transactions.Where(b => b.TransactionHash == hash).Single();
            }

            catch (InvalidOperationException)
            {
                db.Transactions.Remove(tx); //это дичь
                db.SaveChanges();
                throw new InvalidTransactionException("Optimistic concurrency ");
            }
            if (tx == null)
            {
                return null;
            }
            
            Transaction response = Serializer.DeserializeTransaction(tx);

            return await Task.FromResult(response);
        }

       

      

    }
}
