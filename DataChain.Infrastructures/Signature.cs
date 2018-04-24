﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.DataLayer;
using System.Security.Cryptography;

namespace DataChain.Infrastructures
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

        public  bool VerifyMessage(string originalData, string signedDataBase64, string publicKey)
        {
            bool verified;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);

            rsa.FromXmlString(publicKey);
  
            var originalByteData = Encoding.UTF8.GetBytes(originalData);
            var signedData = Convert.FromBase64String(signedDataBase64);

            verified = rsa.VerifyData(originalByteData, CryptoConfig.MapNameToOID("SHA256"), signedData);

            return verified;
        }

        private static HexString SignData(HexString data, string privateKey)
        {
            // Получаем объект класса RSA через провайдер
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);

            // Говорим, что у нас уже есть приватный ключ (например взятый из файла) и следует использовать его
            rsa.FromXmlString(privateKey);

            // Преобразуем символы строки в последовательность байтов   
            byte[] byteData = data.ToByteArray();

            // Хэшируем наши данные с помощью SHA512 и подписываем уже полученный хэш (то есть, берется уже хэш от данных, а не сами данные)
            byte[] signedByteData = rsa.SignData(byteData, CryptoConfig.MapNameToOID("SHA256"));

            // Конвертируем массив байтов в строкове представление в кодировке Base64
            var signedData = new HexString(signedByteData);

            // Возвращаем ЭЦП
            return signedData;
        }
    }
}
