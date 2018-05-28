using System;
using System.Text;
using System.IO;
using DataChain.DataProvider;
using System.Security.Cryptography;


namespace DataChain.Infrastructure
{
    public class ECKeyValidator
    {
        private RSACryptoServiceProvider rsa;

        public RSACryptoServiceProvider RSA
        {
            get
            {
                return rsa;
            }
            set
            {
                rsa = value;
            }
        }

        public ECKeyValidator()
        {
            rsa = new RSACryptoServiceProvider(2048);
        }

        public bool VerifyMessage( string originalData, string signedDataBase64, string publicKey)
        {

            bool verified;

            RSA.FromXmlString(publicKey);

            var originalByteData = Encoding.UTF8.GetBytes(originalData);
            var signedData = Convert.FromBase64String(signedDataBase64);
           
            verified = RSA.VerifyData(originalByteData, CryptoConfig.MapNameToOID("SHA256"), signedData);
            return verified;
        }

       

        public string SignData(string data, string privateKey)
        {

            RSA.FromXmlString(privateKey);
            
            byte[] byteData = Serializer.ToBinaryArray(data);
            byte[] signedByteData = RSA.SignData(byteData, CryptoConfig.MapNameToOID("SHA256"));
            
            var signedData = Convert.ToBase64String(signedByteData);
    
            return signedData;
        }

        public RSACryptoServiceProvider CreateKeys()
        {
           
            var privateKey = RSA.ToXmlString(true);
            var path = "/DataChain/privKey/";
            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            File.WriteAllText(path+"key.xml", privateKey);
            return RSA;
        }

       
    }
}
