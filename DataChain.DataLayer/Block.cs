using System;
using System.Collections.Generic;


namespace DataChain.DataLayer
{
   public class Block
    {
        public int Index { get; set; }
        public HexString Hash { get; set; }
        public string TimeStamp { get; set; }
        public HexString PreviousHash { get; set; }
        public BlockMetadata Metadata { get; set; }

        public Block( HexString _hash, HexString _previousHash, string _timestamp, int _index, BlockMetadata _metadata)
        {
            this.Hash = _hash;
            this.TimeStamp = _timestamp;
            this.Index = _index;
            this.PreviousHash = _previousHash;
            this.Metadata = _metadata;
        }

        public Block()
        {

        }
       
       
    }
}
