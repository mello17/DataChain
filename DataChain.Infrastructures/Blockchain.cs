using System;
using DataChain.DataLayer;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataChain.DataLayer.Interfaces;
using NLog;

namespace DataChain.Infrastructures
{
    public class BlockBuilder
    {
       private IBlockSubscriber subscribe;
       private Logger log;

       public BlockBuilder(IBlockSubscriber _subscribe)
       {
             subscribe = _subscribe;
           
       }

        public Block LatestBlock
        {
            get; set;
        }

        
        public Block CreateGenesis(Block genesis)
        {
            return new Block(  MerkleTree.GetMerkleRoot(genesis.Metadata,1),
                                HexString.Empty,
                                DateTime.Now.ToLocalTime().ToString(),
                                1 , 
                                genesis.Metadata );
        }

        public async Task<Block> GenerateBlock(Block block)
        {

            this.LatestBlock = await subscribe.GetLatestBlock();

            var prevHash = this.LatestBlock.Hash;
            var nextIndex = this.LatestBlock.Index + 1;
            var metaData = ComputeMetadata();
            var nextHash = MerkleTree.GetMerkleRoot(ComputeMetadata(), ComputeMetadata().TransactionCount);
            var timestamp = DateTime.Now.ToLocalTime().ToString();

            return new Block( nextHash, prevHash, timestamp,nextIndex, metaData );
        }

        public string BlockHeader(Block block)
        {
            return String.Concat(block.Index, block.TimeStamp, block.PreviousHash, block.Hash);
        }

        private BlockMetadata ComputeMetadata()
        {
            return new BlockMetadata();
        }

        private HexString ComputeHash( Block previousBlock)
        {
           
            string result = String.Empty;

            foreach(Transaction trans in previousBlock.Metadata.CurrentTransactions)
            {
                result += trans.Hash.ToString();
            }

            return HexString.Parse(result);
        }

        public bool IsValidNewBlock(Block newBlock, Block previousBlock)
        {
            this.log = LogManager.GetCurrentClassLogger();

            if (previousBlock.Index +1 != newBlock.Index)
            {
                log.Error($"Invalid index. Block id : {newBlock.Index}, current block id : {previousBlock.Index} ");
                return false;
            }
            else if (previousBlock.Hash != newBlock.Hash)
            {
                log.Error($"Invalid hash. Block hash : {newBlock.Hash}, current block : {previousBlock.Hash}");
                return false;
            }

            return true;
        }
        
       


    }
}
