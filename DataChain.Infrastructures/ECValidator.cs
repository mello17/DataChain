using System;
using System.Text;
using System.IO;
using DataChain.DataProvider;
using System.Security.Cryptography;


namespace DataChain.Infrastructure
{
    public class ECKeyValidator
    {

        public RSACryptoServiceProvider RSA { get; private set; }

        public ECKeyValidator()
        {
            RSA = new RSACryptoServiceProvider(2048);
        }

        public bool VerifyMessage( string originalData, string signedDataBase64, string publicKey)
        {

            if(string.IsNullOrEmpty(originalData) || string.IsNullOrEmpty(signedDataBase64) || string.IsNullOrEmpty(publicKey))
            {
                throw new ArgumentNullException();
            }

            bool verified;

            RSA.FromXmlString(publicKey);

            var originalByteData = Encoding.UTF8.GetBytes(originalData);
            var signedData = Convert.FromBase64String(signedDataBase64);
           
            verified = RSA.VerifyData(originalByteData, CryptoConfig.MapNameToOID("SHA256"), signedData);
            return verified;
        }

       

        public string SignData(string data, string privateKey)
        {

            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentNullException();
            }

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
