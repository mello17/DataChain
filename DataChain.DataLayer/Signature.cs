using System;


namespace DataChain.DataLayer
{
   public class Signature
    {

        public HexString SignatureData { get; set; }
        public HexString PublicKey { get; set; }

        public Signature( HexString _signature, HexString _publicKey)
        {
            this.SignatureData = _signature;
            this.PublicKey = _publicKey;
        }

        public Signature()
        {

        }

      

    }
}
