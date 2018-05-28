using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using DataChain.Abstractions;
using DataChain.Abstractions.Interfaces;
using DataChain.DataProvider;
using DataChain.WebApplication.Controllers;
using DataChain.Infrastructure;
using DataChain.WebApplication.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace DataChain.Tests
{
    /// <summary>
    /// Сводное описание для UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest
    {
        public UnitTest()
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
       

        public List<Transaction> InitTransactions()
        {
            byte[] data = new HexString(HexString.Parse("abc21321412123131312").ToByteArray()).ToByteArray();
            byte[] data2 = new HexString(HexString.Parse("f213412431313131").ToByteArray()).ToByteArray();
            Record record = new Record(1, "Peace!!!!", new HexString(data), TypeData.Host);
            Transaction tx = new Transaction(DateTime.UtcNow, new List<Record> { record }, new HexString(Serializer.ComputeHash(data)), HexString.Empty, HexString.Empty);
            Transaction tx2 = new Transaction(DateTime.UtcNow, new List<Record> { record }, new HexString(Serializer.ComputeHash(data2)), HexString.Empty, HexString.Empty);
            Transaction tx3 = new Transaction(DateTime.UtcNow, new List<Record> { record }, new HexString(Serializer.ComputeHash(data2)), HexString.Empty, HexString.Empty);
            Transaction tx4 = new Transaction(DateTime.UtcNow, new List<Record> { record }, new HexString(Serializer.ComputeHash(data2)), HexString.Empty, HexString.Empty);
            Transaction tx5 = new Transaction(DateTime.UtcNow, new List<Record> { record }, new HexString(Serializer.ComputeHash(data2)), HexString.Empty, HexString.Empty);

            return new List<Transaction> { tx, tx2, tx3, tx4, tx5 };
        }
        #endregion

        [TestMethod]
        public void TestMerkleRoot()
        {
            SHA256 hasher = SHA256.Create();
            
           

            BlockMetadata meta = new BlockMetadata()
            {
                CurrentTransactions = new List<Transaction>(InitTransactions()) ,
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
            
            Assert.IsNotNull(new ECKeyValidator().CreateKeys());
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

        [TestMethod]
        public  void TestAuthorize()
        {

            UnitOfWork work = new UnitOfWork();
            var controller = new MainController(work);
            Account user = new Account() {Login = "good", Password = new HexString(UTF8Encoding.UTF8.GetBytes("badaps1")) };
            //user.AddUser(user);

             controller.Authentication();
          // Assert.IsFalse();
        }

        [TestMethod]
        public void TestGetLastTx()
        {
            TransactionSubscriber subscriber = new TransactionSubscriber();
            var txs = subscriber.GetLastTransactionAsync();

            Assert.AreEqual(5,txs.Count);
        }

        [TestMethod]
        public  void TestCommitBlock()
        {
            BlockBuilder builder = new BlockBuilder();
            BlockSubscriber subscriber = new BlockSubscriber();
            var block = subscriber.GetBlock(1);
           // builder.CommitBlock(block);
        }

        [TestMethod]
        public  void TestAddingTransactions()
        {
           // var genesis = Genesis.CreateGenesis();
           
            ITransactionSubscriber transactionSubscriber = new TransactionSubscriber();
             transactionSubscriber.AddTransactionAsync(InitTransactions());

        }

        [TestMethod]
        public async Task TestGetTx()
        {
            ITransactionSubscriber transactionSubscriber = new TransactionSubscriber();
            var tx = await transactionSubscriber.GetTransactionAsync(5);

            Assert.IsNotNull(tx);

        }

        [TestMethod]
        public void TestValidateTransactions()
        {
            TransactionSubscriber transactionSubscriber = new TransactionSubscriber();
            TransactionValidator validator = new TransactionValidator(transactionSubscriber);
            string dataString = "String";
            byte[] data = Serializer.ToHexString(dataString);
            var message = "Peace!!!!";
            Record records = new Record(1, message, new HexString(data), TypeData.Host);
            
            var jArray = JArray.FromObject( new List<Record>(){ records });
            string privkey = new ECKeyValidator().CreateKeys().ToXmlString(true);
            string pubkey =new ECKeyValidator().CreateKeys().ToXmlString(false);
            new ECKeyValidator().SignData(message, privkey);
            validator.ValidateTransaction(jArray, pubkey);
            
        }

        [TestMethod]
        public void TestCreateBlock()
        {
            // 
            BlockBuilder builder = new BlockBuilder();
           // var genesis = Genesis.CreateGenesis();
            BlockSubscriber subscriber = new BlockSubscriber();
           // 
             var block =  builder.GenerateBlock(InitTransactions());
             builder.AddBlock(block);
            Assert.IsNotNull(block);
        }

        [TestMethod]
        public void TestGetBlock()
        {
            IBlockSubscriber subscriber = new BlockSubscriber();

            IEnumerable<Block> blocks =  subscriber.GetBlocks();
        }

        [TestMethod]
        public void TestSerialize()
        {
            ECKeyValidator keyValidator = new ECKeyValidator();
            var priv_key = keyValidator.RSA.ToXmlString(true);
            var pub_key = keyValidator.RSA.ToXmlString(false);
            var sign = keyValidator.SignData("messadhgdhdhh", priv_key);
            SignatureEvidence rawSign = new SignatureEvidence(new HexString(Serializer.ToBinaryArray(sign)),
                new HexString(Serializer.ToBinaryArray(pub_key)));
            ITransactionSubscriber subscr = new TransactionSubscriber();
            TransactionValidator validator = new TransactionValidator(subscr);
            var byteSign = validator.SerializeSignature(rawSign);

            byte[] bytePrivKey = null, bytePubKey = null;
            using (MemoryStream stream1 = new MemoryStream(byteSign))
            {
                using (BinaryReader reader = new BinaryReader(stream1)) {

                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    bytePubKey = reader.ReadBytes(243);
                    bytePrivKey = reader.ReadBytes(178);
                }
            }

            var pub = Serializer.ToBinaryArray(pub_key);
            var priv = Serializer.ToBinaryArray(sign);
           
            Assert.ReferenceEquals(pub, bytePubKey);

        }

        [TestMethod]
        public void TestChainEncode()
        {
            IBlockSubscriber subscriber = new BlockSubscriber();
            var block =  subscriber.GetBlock(10);

           // Chain chain = new Chain(block);

            ChainSerializer serializer = new ChainSerializer();
           // Assert.IsNotNull(serializer.Encode(chain.BlockChain));
        }




    }
}
