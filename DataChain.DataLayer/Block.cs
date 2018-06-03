using System;


namespace DataChain.Abstractions
{
   public class Block
    {
        public int Index { get; }
        public HexString Hash { get; }
        public DateTime TimeStamp { get; }
        public HexString PreviousHash { get; }
        public HexString MerkleRoot { get; }
       

        public Block( HexString _hash, HexString _previousHash, DateTime _timestamp, int _index, HexString _merkle)
        {
            this.Hash = _hash;
            this.MerkleRoot = _merkle;
            this.TimeStamp = _timestamp;
            this.Index = _index;
            this.PreviousHash = _previousHash;
           
        }

        public Block()
        {

        }
       
       
    }
}
