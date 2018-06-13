using System;


namespace DataChain.Abstractions
{
   public class SignatureEvidence
    {

        public HexString SignatureData { get; set; }
        public HexString PublicKey { get; set; }

        public SignatureEvidence(HexString _signature, HexString _publicKey)
        {
            this.SignatureData = _signature;
            this.PublicKey = _publicKey;
        }

        public SignatureEvidence()
        {

        }

      

    }
}
