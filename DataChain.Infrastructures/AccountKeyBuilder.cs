using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataChain.Abstractions;

namespace DataChain.Infrastructure
{
    public  class AccountKeyBuilder
    {
        
        private static readonly ECKeyValidator eckey;

        static AccountKeyBuilder()
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
