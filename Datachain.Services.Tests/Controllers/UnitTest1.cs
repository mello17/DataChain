using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataChain.Infrastructures;
using System.Security.Cryptography;
using DataChain.DataLayer;
using DataChain.EntityFramework;

namespace Datachain.Services.Tests.Controllers
{
    /// <summary>
    /// Сводное описание для UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            //
            // TODO: добавьте здесь логику конструктора
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты тестирования
        //
        // При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        // ClassInitialize используется для выполнения кода до запуска первого теста в классе
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // TestInitialize используется для выполнения кода перед запуском каждого теста 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // TestCleanup используется для выполнения кода после завершения каждого теста
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        [TestInitialize]
        public void InitTransactions()
        {

        }
        #endregion

        [TestMethod]
        public void TestMerkleRoot()
        {
            SHA256 hasher = SHA256.Create();
            byte[] data= new HexString(HexString.Parse("abc21321412123131312").ToByteArray()).ToByteArray();
            byte[] data2 = new HexString(HexString.Parse("f213412431313131").ToByteArray()).ToByteArray();
            Transaction tx = new Transaction(DateTime.UtcNow, null,new HexString(Serializer.ComputeHash(data)), HexString.Empty);
            Transaction tx2 = new Transaction( DateTime.UtcNow, null, new HexString(Serializer.ComputeHash(data2)), HexString.Empty);
            Transaction tx3 = new Transaction( DateTime.UtcNow, null, new HexString(Serializer.ComputeHash(data2)), HexString.Empty);
            Transaction tx4 = new Transaction( DateTime.UtcNow, null, new HexString(Serializer.ComputeHash(data2)), HexString.Empty);
            Transaction tx5 = new Transaction( DateTime.UtcNow, null, new HexString(Serializer.ComputeHash(data2)), HexString.Empty);

            BlockMetadata meta = new BlockMetadata()
            {
                CurrentTransactions = new List<Transaction>() { tx, tx2, tx3, tx4, tx5 },
                Instance = 1,
                TransactionCount = 5

            };

            Block block = new Block()
            {
                Metadata = meta
            };
            var root = MerkleTree.GetMerkleRoot(meta, meta.TransactionCount);
            Assert.IsNotNull(root);
        }
        
        [TestMethod]
        public void TestSign()
        {
            byte[] data = HexString.Parse("abc21321412123131312").ToByteArray();
            byte[] str  = UTF8Encoding.UTF8.GetBytes("abc21321412123131312");
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
            string privKey = rsa.ToXmlString(true);
            
            Assert.IsNotNull(new ECKeyValidator().SignData(data.ToString(), privKey));

        }

        [TestMethod]
        public void TestKeys()
        {
            new ECKeyValidator().CreateKeys();
            Assert.Fail();
        }

        [TestMethod]
        public void TestECK()
        {
            ECKeyValidator key =  new ECKeyValidator();
            string publicKey = key.RSA.ToXmlString(false);
            string privateKey = key.RSA.ToXmlString(true);

            string plainText = "originalMessage";
            // string tamperMessage = "origiinalMessage";

            string signedMessage = key.SignData( plainText,privateKey);

            Assert.IsTrue(key.VerifyMessage(plainText, signedMessage, publicKey));

        }
    }
}
