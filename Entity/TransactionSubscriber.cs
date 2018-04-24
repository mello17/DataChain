using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.Infrastructures;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;

namespace DataChain.EntityFramework
{
   public class TransactionSubscriber : ITransactionSubscriber
    {
        private DatachainContext db = new DatachainContext();


        public async Task<Transaction> GetLastTransactionAsync()
        {
           var last = db.Transactions.Max(b=>b.Id);
            if (last == 0)
            {
                throw new InvalidBlockException("Transaction is empty");
            }

            return await GetTransactionAsync((uint)last);
        }

        public async Task AddTransactionAsync(IEnumerable<Transaction> transactions)
        {
            IList<TransactionModel> tx_list = new List<TransactionModel>();
            foreach (var tx in transactions)
            {
                var list = tx.Data.ToList();
                string result = String.Empty;
                foreach (var data in list)
                {

                  result +=  data.Value.ToString();
                }

                
                TransactionModel model = new TransactionModel();
                model.TransactionHash = Serializer.ComputeHash( result.ToHexString());
                model.RawData = HexString.Parse(result).ToByteArray();
                model.Timestamp = tx.TimeStamp;
                tx_list.Add(model);
            }

            db.Transactions.AddRange(tx_list);
            await db.SaveChangesAsync();
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

      

    }
}
