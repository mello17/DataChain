using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;

namespace DataChain.EntityFramework
{
   public class TransactionSubscriber : ITransactionSubscriber
    {
        private DatachainContext db = new DatachainContext();
        private IBlockSubscriber blcSubscribe = new BlockSubscriber();


        public async Task<Transaction> GetLastTransactionAsync()
        {
            var last = db.Transactions.Max(b=>b.Id);
            db.Transactions.Include("Block");
            var tx = db.Transactions.Last();

            if (last == 0)
            {
                throw new InvalidBlockException("Transaction is empty");
            }

            return await GetTransactionAsync((uint)last);
        }

        public async Task<byte[]> AddTransactionAsync(IEnumerable<Transaction> transactions)
        {
            IList<TransactionModel> tx_list = new List<TransactionModel>();
            string result = null;

            foreach (var tx in transactions)
            {

                var list = tx.Data.Select(s =>s.Value.ToString()).ToList();

                result = Serializer.ConcatenateData(list);

                var model = new TransactionModel()
                {
                    TransactionHash = tx.Hash.ToByteArray(),
                    RawData = Serializer.ToBinaryArray( result ),
                    Timestamp = tx.TimeStamp
                };
                tx_list.Add(model);
            }

            db.Transactions.AddRange(tx_list);
            await db.SaveChangesAsync();

            return Serializer.ComputeHash(Serializer.ToBinaryArray(result));
        }

        [global::System.Diagnostics.Contracts.ContractRuntimeIgnored]
        public async Task<Transaction> GetTransactionAsync(uint index)
        {
            var tx = await db.Transactions.FindAsync(index);

            if (tx == null)
            {
                throw new InvalidTransactionException("Transaction cannot find");
            }

            Transaction response = Serializer.DeserializeTransaction(tx);

            return response;

        }

        public async Task<Transaction> GetTransactionAsync(byte[] hash)
        {
            var tx = db.Transactions.Where(b=>b.TransactionHash == hash).SingleOrDefault();

            if (tx == null)
            {
                throw new InvalidTransactionException("Transaction cannot find");
            }
            
            Transaction response = Serializer.DeserializeTransaction(tx);

            return await Task.FromResult(response);
        }

       

      

    }
}
