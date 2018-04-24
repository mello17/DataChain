using System.Collections.Generic;

namespace DataChain.DataLayer
{
   public class BlockMetadata
    {
        public int Instance { get; set; }
        public  int TransactionCount
        {
            get;set;          
        }
        public IList<Transaction> CurrentTransactions { get; set; }

        public Transaction LatestTransaction
        {
            get; 
        }

       
    }
}
