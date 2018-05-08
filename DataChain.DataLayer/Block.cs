﻿using System;
using System.Collections.Generic;


namespace DataChain.DataLayer
{
   public class Block
    {
        public int Index { get; }
        public HexString Hash { get; }
        public DateTime TimeStamp { get; }
        public HexString PreviousHash { get; }
        public HexString MerkleRoot { get; }
        public BlockMetadata Metadata { get; set; }

        public Block( HexString _hash, HexString _previousHash, DateTime _timestamp, int _index, BlockMetadata _metadata)
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
