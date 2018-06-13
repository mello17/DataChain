using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using DataChain.Abstractions;
using DataChain.Abstractions.Interfaces;
using System.Data.Entity;

namespace DataChain.DataProvider
{
   public class TransactionRepository : ITransactionRepository
    {
        private readonly DatachainContext database ;
        private readonly IBlockRepository blcSubscribe;

        public TransactionRepository()
        {
            database = new DatachainContext();
            blcSubscribe = new BlockRepository();
        }


        public  List<Transaction> GetLastTransactionAsync()
        {

            IEnumerable<TransactionModel> tx_list;
            try
            {
                tx_list = database.Transactions.Where(x => !(x.BlockModelId.HasValue) );
              
                
            }
            
            catch (InvalidOperationException ex)
            {
                throw new InvalidTransactionException(ex.Message);
            }

            var raw_tx_list = new List<Transaction>();
            foreach(var tx in tx_list)
            {
                raw_tx_list.Add(Serializer.DeserializeTransaction(tx));
            }
            

            return raw_tx_list;
        }

        public void Update(List<Transaction> raw_tx, int index)
        {

            List<TransactionModel> serialized_tx = new List<TransactionModel>();

            foreach (var tx in raw_tx)
            {
                serialized_tx.Add( Serializer.SerializeTransaction(tx));
            }
            try
            {
               
                    foreach (var tx in serialized_tx)
                    {
                        if (tx.BlockModelId == null)
                        {
                            var original = database.Transactions.Find(tx.Id);
                            original.BlockModelId = index;
                            
                            database.SaveChanges();
                        }
                    }
            }
            catch (DbEntityValidationException ex)
            {
                throw new InvalidOperationException(ex.EntityValidationErrors.First()
                    .ValidationErrors
                    .FirstOrDefault()
                    .ErrorMessage);
            }
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

               var txs = database.Transactions.ToList();
               foreach(var raw_tx in txs)
                {
                    if(raw_tx.TransactionHash.SequenceEqual(model.TransactionHash)  )
                    {
                        throw new InvalidTransactionException("Optimistic concurrency");
                    }
                }

                tx_list.Add(model);
            }


            try
            {

                database.Transactions.AddRange(tx_list);
                await database.SaveChangesAsync();
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
            var tx = await database.Transactions.FindAsync(index);

            if (tx == null)
            {
                return null;
            }

            Transaction response = Serializer.DeserializeTransaction(tx);

            return response;

        }


        public List<Transaction> GetTransactionsByBlockId(int id)
        {

            List<TransactionModel> tx_list = new List<TransactionModel>();
            IEnumerable<Transaction> raw_tx_list = null;

            try
            {
                tx_list = database.Transactions.Where(b => b.BlockModelId == id)
                    .ToList();
                raw_tx_list = Serializer.TransactionsMapping(tx_list, id);
            }
            catch 
            {
                throw new InvalidTransactionException("");
            }

            return raw_tx_list.ToList();
        }
        public async Task<Transaction> GetTransactionAsync(byte[] hash)
        {
            TransactionModel tx = null; 

            try
            {
                 tx = database.Transactions.Where(b => b.TransactionHash == hash).First();
                if (tx == null)
                {
                    return null;
                }
            }

            catch (InvalidOperationException)
            {
                database.Transactions.Remove(tx); //это дичь
                database.SaveChanges();
                throw new InvalidTransactionException("Optimistic concurrency ");
            }
            
            
            Transaction response = Serializer.DeserializeTransaction(tx);

            return await Task.FromResult(response);
        }

       

      

    }
}
