using System.Collections.Generic;

namespace DataChain.Abstractions
{
   public class BlockMetadata
    {
        public int Instance { get; set; }
        public int TransactionCount
        {
            get;set;          
        }
        public IList<Transaction> CurrentTransactions { get; set; }

       
    }
}
