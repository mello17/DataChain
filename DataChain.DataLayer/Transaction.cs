using System;
using System.Collections.Generic;



namespace DataChain.Abstractions
{

    [Serializable]
    public class Transaction
    {
        public Transaction( DateTime _timestamp, IEnumerable<Record>  _data, HexString _hash, HexString _pubkey, HexString _sign)
        {
         
            if(_timestamp == null)
            {
                throw new ArgumentNullException(nameof(_timestamp));
            }  

            this.TimeStamp = _timestamp ;
            this.Data = _data ?? throw new ArgumentNullException(nameof(_data));
            this.Hash = _hash ?? throw new ArgumentNullException(nameof(_hash));
            this.Sign = _sign ?? throw new ArgumentNullException(nameof(_sign));
            this.PubKey = _pubkey ?? throw new ArgumentNullException(nameof(_pubkey));
        }

        public Transaction()
        {

        }

        public int Instance { get; set; }
        public DateTime TimeStamp { get; set; }
        public IEnumerable<Record> Data { get; set; }
        public HexString Hash { get; set; }
        public HexString Sign { get; set; }
        public HexString PubKey { get; set; }

        public Transaction Clone(Transaction other)
        {
            return new Transaction(other.TimeStamp, other.Data, other.Hash, other.Sign, other.PubKey);
        }
    }
}
