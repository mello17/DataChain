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

            if(string.IsNullOrEmpty(originalData) || string.IsNullOrEmpty(signedDataBase64))
            {
                throw new ArgumentNullException();
            }
            
            var par =  RSA.ExportParameters(false);
            RSA.ImportParameters(par);
            //RSA.FromXmlString(publicKey);
            
            var originalByteData = Encoding.UTF8.GetBytes(originalData);
            var signedData = Convert.FromBase64String(signedDataBase64);
            
            return RSA.VerifyData(originalByteData, CryptoConfig.MapNameToOID("SHA256"), 
                signedData);
        }

       

        public string SignData(string data)
        {

            if (string.IsNullOrEmpty(data) )
            {
                throw new ArgumentNullException();
            }

            var par = RSA.ExportParameters(true);
            
            RSA.ImportParameters(par);

            byte[] byteData = Serializer.ToBinaryArray(data);
            byte[] signedByteData = RSA.SignData(byteData, CryptoConfig.MapNameToOID("SHA256"));
    
            return Convert.ToBase64String(signedByteData);
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
