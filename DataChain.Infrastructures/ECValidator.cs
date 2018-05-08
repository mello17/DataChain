using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using DataChain.DataLayer;
using DataChain.EntityFramework;
using System.Security.Cryptography;


namespace DataChain.Infrastructures
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
            rsa = new RSACryptoServiceProvider(1024);
        }

        public bool VerifyMessage( string originalData, string signedDataBase64, string publicKey)
        {
            bool verified;

            RSA.FromXmlString(publicKey);

            var originalByteData = Encoding.UTF8.GetBytes(originalData);
            var signedData = Convert.FromBase64String(signedDataBase64);
            RSA.Encrypt(originalByteData, false);
            verified = RSA.VerifyData(originalByteData, CryptoConfig.MapNameToOID("SHA256"), signedData);

            return verified;
        }

        public string SignData(string data, string privateKey)
        {

            RSA.FromXmlString(privateKey);

            // Преобразуем символы строки в последовательность байтов   
            byte[] byteData = Serializer.ToBinaryArray(data);

            byte[] signedByteData = RSA.SignData(byteData, CryptoConfig.MapNameToOID("SHA256"));
            
            var signedData = Convert.ToBase64String(signedByteData);//new HexString(signedByteData);

            // Возвращаем ЭЦП
            return signedData;
        }

        public RSACryptoServiceProvider CreateKeys()
        {
           
            var privateKey = RSA.ToXmlString(true);
            File.WriteAllText("privatekey.xml", privateKey);
            return RSA;
        }

       
    }
}
