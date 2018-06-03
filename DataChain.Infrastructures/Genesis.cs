using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DataChain.Abstractions;

namespace DataChain.Infrastructure
{
   public static class Genesis
    {
        public static Block GenesisBlock { get; private set; }

        static Genesis()
        {
            CreateGenesis();
        }

        public static Block CreateGenesis()
        {

            GenesisBlock = new Block(new HexString(HexString.Parse("00").ToByteArray()), new HexString(HexString.Parse("00").ToByteArray()), DateTime.UtcNow, 1, new HexString(HexString.Parse("00").ToByteArray()));

            string jsondata = JsonConvert.SerializeObject(GenesisBlock);
            string path = "/DataChain/Genesis/";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            File.WriteAllText(path+"genesis.json", jsondata);

            return GenesisBlock;
            

        }

    }
}
