using System;
using System.Collections.Generic;


namespace DataChain.DataLayer
{
    public class Transaction
    {
        public Transaction(int _instance, string _timestamp, IEnumerable<Record>  _data, HexString _hash)
        {
            this.Instance = _instance;
            this.TimeStamp = _timestamp;
            this.Data = _data;
            this.Hash = _hash;
        }


        public int Instance { get; set; }
        public string TimeStamp { get; set; }
        public IEnumerable<Record> Data { get; set; }
        public HexString Hash { get; set; }

        public Transaction Clone(Transaction other)
        {
            return new Transaction(other.Instance,other.TimeStamp, other.Data, other.Hash);
        }
    }
}
