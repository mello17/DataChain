using System;
using System.Collections.Generic;



namespace DataChain.DataLayer
{
    public class Transaction
    {
        public Transaction( DateTime _timestamp, IEnumerable<Record>  _data, HexString _hash, HexString _sign)
        {
         
            this.TimeStamp = _timestamp;
            this.Data = _data;
            this.Hash = _hash;
            this.Sign = _sign;
        }


        public int Instance { get; set; }
        public DateTime TimeStamp { get; set; }
        public IEnumerable<Record> Data { get; set; }
        public HexString Hash { get; set; }
        public HexString Sign { get; set; }

        public Transaction Clone(Transaction other)
        {
            return new Transaction(other.TimeStamp, other.Data, other.Hash, other.Sign);
        }
    }
}
