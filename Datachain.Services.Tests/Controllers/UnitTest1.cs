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
using System.Runtime.Serialization.Formatters.Binary;

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
            byte[] data2 = new HexString(HexString.Parse("f2134124313323213131").ToByteArray()).ToByteArray();
            Record record = new Record(1, "Peace!!!!", new HexString(data), TypeData.Host);
            Transaction tx = new Transaction(DateTime.UtcNow, new List<Record> { record }, new HexString(Serializer.ComputeHash(data)), HexString.Empty, HexString.Empty);
            Transaction tx2 = new Transaction(DateTime.UtcNow, new List<Record> { record }, new HexString(Serializer.ComputeHash(data2)), HexString.Empty, HexString.Empty);
           

            return new List<Transaction> { tx, tx2 };
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
                TransactionCount = 2

            };

            
            var root = MerkleTree.GetMerkleRoot(meta, meta.TransactionCount);
            Assert.IsNotNull(root);
        }
        
        [TestMethod]
        public void TestSign()
        {
            byte[] data = HexString.Parse("abc21321412123131312").ToByteArray();
            
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            string privKey = rsa.ToXmlString(true);
            
            Assert.IsNotNull(new ECKeyValidator().SignData(data.ToString()));

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

            string signedMessage = key.SignData( plainText);

            Assert.IsTrue(key.VerifyMessage(plainText, signedMessage, publicKey));

        }

        [TestMethod]
        public void TestCorrectChain()
        {

            ChainConnector connector = new ChainConnector();
            Record record = new Record(5, "ds", new HexString("323".ToHexString()), TypeData.Host);
            byte[] data2 = new HexString(HexString.Parse("f2134124313323213131").ToByteArray()).ToByteArray();
            Transaction tx2 = new Transaction(DateTime.UtcNow, new List<Record> { record }, new HexString(Serializer.ComputeHash(data2)), HexString.Empty, HexString.Empty);
            BlockBuilder builder = new BlockBuilder(new BlockRepository(), 
                new TransactionRepository());
            var block = builder.GenerateBlock(new List<Transaction>(){ tx2 });
            var correct = connector.CheckCorrect(block);

            Assert.IsTrue(correct);
        }

        [TestMethod]
        public void TestGetBlocks()
        {
            BlockRepository repository = new BlockRepository();
            var blocks = repository.GetBlocks();
        }


        [TestMethod]
        public void TestGetLastTx()
        {
            TransactionRepository subscriber = new TransactionRepository();
            var txs = subscriber.GetLastTransactionAsync();

            Assert.AreEqual(3,txs.Count);
        }

        [TestMethod]
        public  void TestCommitBlock()
        {
            BlockRepository subscriber = new BlockRepository();
            BlockBuilder builder = new BlockBuilder(subscriber, new TransactionRepository());
            
            var block = subscriber.GetBlock(62).Result;

            builder.CommitBlock(block).Wait();
            Assert.IsNotNull(block);
        }

        [TestMethod]
        public void TestLastTransaction()
        {
            TransactionRepository subscriber = new TransactionRepository();
            var list = subscriber.GetLastTransactionAsync();
        }

        [TestMethod]
        public  void TestAddingTransactions()
        {
           
            ITransactionRepository transactionSubscriber = new TransactionRepository();
             transactionSubscriber.AddTransactionAsync(InitTransactions()).Wait();

        }

        [TestMethod]
        public async Task TestGetTx()
        {
            ITransactionRepository transactionSubscriber = new TransactionRepository();
            var tx = await transactionSubscriber.GetTransactionAsync(5);

            Assert.IsNull(tx);

        }

        [TestMethod]
        public void TestValidateTransactions()
        {
           
            TransactionValidator validator = new TransactionValidator();
            string dataString = "String";
            byte[] data = Serializer.ToHexString(dataString);
            var message = "Peace!!!!";
            AccountKeyBuilder keyBuilder = new AccountKeyBuilder();
            Account newAcc;
            using (var hasher = SHA256.Create())
            {
                newAcc = new Account()
                {
                    Key = keyBuilder.CreateAccKey(),
                    Login = message,
                    Password = new HexString(hasher.ComputeHash
                   (Serializer.ToBinaryArray("212121"))),
                    Role = UserRole.Admin
                };
            }
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, newAcc);
            Record record = new Record(1, "New acc " + newAcc.Login,
                new HexString(stream.ToArray()), TypeData.Account);
           
            Record records = new Record(1, message, new HexString(data), TypeData.Host);
            
            var jArray = JArray.FromObject( new List<Record>(){ records });
           // string privkey = new ECKeyValidator().CreateKeys().ToXmlString(true);
            string pubkey =new ECKeyValidator().RSA.ToXmlString(false);
            
            validator.ValidateTransaction(record, pubkey).Wait();
            
        }

        [TestMethod]
        public void TestCreateBlock()
        {
            BlockRepository subscriber = new BlockRepository();

            BlockBuilder builder = new BlockBuilder(subscriber, new TransactionRepository());
          
             var block =  builder.GenerateBlock(InitTransactions());
            
            Assert.IsNotNull(block);
        }

        [TestMethod]
        public void TestGetBlock()
        {
            IBlockRepository subscriber = new BlockRepository();

            IEnumerable<Block> blocks =  subscriber.GetBlocks();

            Assert.IsNotNull(blocks);
        }

        [TestMethod]
        public void TestSerialize()
        {
            ECKeyValidator keyValidator = new ECKeyValidator();
            var priv_key = keyValidator.RSA.ToXmlString(true);
            var pub_key = keyValidator.RSA.ToXmlString(false);
            var sign = keyValidator.SignData("messadhgdhdhh");
            SignatureEvidence rawSign = new SignatureEvidence(new HexString(Serializer.ToBinaryArray(sign)),
                new HexString(Serializer.ToBinaryArray(pub_key)));
            ITransactionRepository subscr = new TransactionRepository();
            TransactionValidator validator = new TransactionValidator();
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
            IBlockRepository subscriber = new BlockRepository();
            var blocks =  subscriber.GetBlocks();
            
            Chain chain = new Chain(blocks);

            ChainSerializer serializer = new ChainSerializer();
            var encrypt = serializer.Encode(chain.BlockChain);
            var decrypt = serializer.Decode(serializer.ConcateByteArray(encrypt));

            Assert.AreEqual(((List<Block>)decrypt).Count, chain.BlockChain.Count);
        }

        [TestMethod]
        public void TestGenesisEncode()
        {

            Chain chain = new Chain(Genesis.CreateGenesis());

            ChainSerializer serializer = new ChainSerializer();

            var encrypt = serializer.Encode(chain.BlockChain);
            var decrypt = serializer.Decode(serializer.ConcateByteArray(encrypt));

            Assert.AreEqual(((List<Block>)decrypt).Count, chain.BlockChain.Count);
        }

        [TestMethod]
        public void TestSerializeTx()
        {
            Record record = new Record(1, "3242", new HexString("http://localhost:8080/".ToHexString()), TypeData.Host);
            Transaction transaction = new Transaction(DateTime.UtcNow, new[] { record },
               new HexString(Serializer.ConcatenateData(new[] { "http://localhost:8080/", "123" }).ToHexString()),
               new HexString(new ECKeyValidator().RSA.ToXmlString(false).ToHexString()),
               new HexString(new ECKeyValidator().SignData("http://localhost:8080/").ToHexString()));

            var model = Serializer.SerializeTransaction(transaction);
            var result_tx = Serializer.DeserializeTransaction(model);

            Assert.IsTrue(transaction.Hash.Equals(result_tx.Hash));
            Assert.IsTrue(transaction.PubKey.Equals( result_tx.PubKey));
            Assert.IsTrue(transaction.Sign.Equals( result_tx.Sign));
            Assert.AreEqual(transaction.TimeStamp,  result_tx.TimeStamp);
           // Assert.AreEqual(transaction.Data, result_tx.Data);

        }
    }
}
