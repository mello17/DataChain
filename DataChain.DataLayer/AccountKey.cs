using System;

namespace DataChain.Abstractions
{

    [Serializable]
    public class AccountKey
    {
        public HexString Key { get; private set; }


        public AccountKey(HexString _key)
        {
            Key = _key;
        }
    }
}
