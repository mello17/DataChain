using System;
using DataChain.DataLayer;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DataChain.DataLayer.Interfaces;
using DataChain.EntityFramework;
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
                                DateTime.UtcNow,
                                0 , 
                                genesis.Metadata );
        }

        public async Task<Block> GenerateBlock(Block block, IEnumerable<Transaction> tx)
        {

            this.LatestBlock = await subscribe.GetLatestBlock();

            var prevHash = this.LatestBlock.Hash;
            var nextIndex = this.LatestBlock.Index + 1;
            var metaData = ComputeMetadata();
            var nextHash = ComputeBlockHash(block);
            var merkleroot = MerkleTree.GetMerkleRoot(ComputeMetadata(), ComputeMetadata().TransactionCount);
            var timestamp = DateTime.UtcNow;

            return new Block( nextHash, prevHash, timestamp,nextIndex, metaData );
        }



        public string CalculateBlockHeader(Block block)
        {
            return string.Concat(block.Index, block.TimeStamp, block.PreviousHash, block.Hash);
        }

        private BlockMetadata ComputeMetadata()
        {
            return new BlockMetadata();
        }

        private HexString ComputeBlockHash( Block previousBlock)
        {

            var header = CalculateBlockHeader(previousBlock);
            return new HexString( Serializer.ComputeHash( Serializer.ToBinaryArray(header)));
        }

        public bool IsValidNewBlock(Block newBlock, Block previousBlock)
        {
            this.log = LogManager.GetCurrentClassLogger();

            if (previousBlock.Index +1 != newBlock.Index)
            {
                log.Error($"Invalid index. Block id : {newBlock.Index}, current block id : {previousBlock.Index} ");
                return false;
            }
            else if (previousBlock.Hash != newBlock.PreviousHash)
            {
                log.Error($"Invalid hash. Block hash : {newBlock.Hash}, current block : {previousBlock.Hash}");
                return false;
            }
            else if (previousBlock.TimeStamp.Millisecond < newBlock.TimeStamp.Millisecond)
            {
                log.Error("Invalid timestamp. New block cannot create in future");
                return false;
            }

            return true;
        }
        
       


    }
}
