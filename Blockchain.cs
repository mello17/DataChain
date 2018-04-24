using System;
using NBitcoin;

namespace DataChain.Infrastructure
{
    public class Blockchain
    {

       public Blockchain()
        {
            var key = new Key();
            var secret= key.GetBitcoinSecret(Network.TestNet);

            PubKey publicKey = key.PubKey;
            BitcoinPubKeyAddress bitcoinPublicKey = publicKey.GetAddress(Network.TestNet);
            var arr = Network.Main._MagicBytes;
           
        } 

        public Transaction GetGenesisTransaction()
        {
            return Network.Main.GetGenesis().Transactions[0];
        }


    }
}
