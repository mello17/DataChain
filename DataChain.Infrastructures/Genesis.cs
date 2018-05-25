using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DataChain.DataLayer;

namespace DataChain.Infrastructures
{
   public static class Genesis
    {
        private static Block genesis;
        public static Block GenesisBlock {
            get
            {
                return genesis;
            }
           private set
            {
                genesis = value;
            }

        }

        static Genesis()
        {
            CreateGenesis();
        }

        public static Block CreateGenesis()
        {

            BlockMetadata Metadata = new BlockMetadata()
            {
                CurrentTransactions = new List<Transaction>(),
                TransactionCount = 0,
                Instance = 1
            };
            GenesisBlock = new Block(new HexString(HexString.Parse("00").ToByteArray()), new HexString(HexString.Parse("00").ToByteArray()), DateTime.UtcNow, 0, new HexString(HexString.Parse("00").ToByteArray()),  Metadata);

            string jsondata = JsonConvert.SerializeObject(genesis);
            string path = "/DataChain/Genesis/";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            File.WriteAllText(path+"genesis.json", jsondata);

            return genesis;
            

        }

    }
}
