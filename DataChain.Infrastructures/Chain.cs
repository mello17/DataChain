using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using DataChain.Abstractions;
using DataChain.Abstractions.Interfaces;
using DataChain.DataProvider;
using System.Net.Http.Headers;
using Newtonsoft;
using System.IO;

namespace DataChain.Infrastructure
{
   public class Chain
    {

        protected List<Block> blockChain = new List<Block>();

        public List<Block> BlockChain { get; set; } = new List<Block>();

        public IEnumerable<Account> Users { get; set; }
        public IEnumerable<string> Hosts { get; set; }
        public int Length => blockChain.Count;
        public IBlockSubscriber BlockSubscribe { get; private set; } = new BlockSubscriber();
       


        public Chain(List<Block> blocks)
        {
            if (blocks == null)
            {
                throw new InvalidBlockException( "Список блоков провайдера данных не может быть равным null.");
            }

            foreach (var block in blocks)
            {
                var b = new Block(block.Hash, block.PreviousHash, block.TimeStamp, block.Index, block.MerkleRoot, block.Metadata );
                BlockChain.Add(b);

            } 
        }

        public Chain(Block block)
        {

            if (block == null)
            {
                throw new InvalidBlockException("Блок провайдера данных не может быть равным null.");
            }

            var b = new Block(block.Hash, block.PreviousHash, block.TimeStamp, block.Index, block.MerkleRoot, block.Metadata);
            BlockChain.Add(b);

           
        }

        public Chain()
        {
           
        }
      

        public void AddBlock(Block block)
        {
           
            if (blockChain.Any(b => b.Hash == block.Hash))
            {
                return;
            }

            blockChain.Add(block);
        }

      
        /// <summary>
        /// Проверить корректность цепочки блоков.
        /// </summary>
        /// <returns> Корректность цепочки блоков. true - цепочка блоков корректна, false - цепочка некорректна. </returns>
        public bool CheckCorrect()
        {

            new Chain( BlockSubscribe.GetBlocks().ToList());
            for (int i = 0; i <= blockChain.Count; i += 2)
            {
                if (i + 1 < blockChain.Count)
                {
                    if(blockChain[i].PreviousHash != blockChain[i + 1].Hash)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

       

        /// <summary>
        /// Получить данные из локальной цепочки.
        /// </summary>
        /// <param name="localChain"> Локальная цепочка блоков. </param>
        public void LoadDataFromLocalChain(Chain localChain)
        {
            if (localChain == null)
            {
                throw new InvalidBlockException("Локальная цепочка блоков не может быть равна null.");
            }

            foreach (var block in localChain.blockChain)
            {
                blockChain.Add(block);
                

            }
        }

    }
}
