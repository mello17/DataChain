using System;
using System.Collections.Generic;


namespace DataChain.Abstractions
{

   [Serializable]
   public class Block
    {
        public int Index { get; }
        public HexString Hash { get; }
        public DateTime TimeStamp { get; }
        public HexString PreviousHash { get; }
        public HexString MerkleRoot { get; }
        public IList<Transaction> CurrentTransactions { get; }


        public Block(HexString _hash, HexString _previousHash, DateTime _timestamp, int _index, 
            HexString _merkle, IList<Transaction> _transactions)
        {
            this.Hash = _hash;
            this.MerkleRoot = _merkle;
            this.TimeStamp = _timestamp;
            this.Index = _index;
            this.PreviousHash = _previousHash;
            this.CurrentTransactions = _transactions;
           
        }

       
       
       
    }
}
