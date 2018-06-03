using DataChain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.Infrastructure
{
    public  class AccountKey
    {
        private readonly HexString key;
        private static readonly ECKeyValidator eckey;

        public AccountKey(HexString _key)
        {
            key = _key;
        }

        static AccountKey()
        {
            eckey = new ECKeyValidator();
        }

        public static byte[] Encode(byte[] rawKey) => eckey.RSA.Encrypt(rawKey, false);

        public static HexString Decode(byte[] decodeKey) =>  new HexString(eckey.RSA.Decrypt(decodeKey, false));

        public AccountKey CreateAccKey()
        {

            byte[] arr = new byte[16];
            RandomNumberGenerator rand = new RNGCryptoServiceProvider();
            rand.GetNonZeroBytes(arr);
            HexString randKey = new HexString(arr);

            return new AccountKey(randKey);
        }
    }
}
