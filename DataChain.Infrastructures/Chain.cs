using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using DataChain.Abstractions;
using DataChain.Abstractions.Interfaces;
using DataChain.DataProvider;

namespace DataChain.Infrastructure
{
   public class Chain
    {

        protected List<Block> blockChain = new List<Block>();

        public List<Block> BlockChain { get; set; } = new List<Block>();

        public IEnumerable<Account> Users { get; set; }
        public IEnumerable<string> Hosts { get; set; }
        public int Length => BlockChain.Count;
        public IBlockRepository BlockSubscribe { get; private set; } = new BlockRepository();
       


        public Chain(List<Block> blocks)
        {
            if (blocks == null)
            {
                throw new InvalidBlockException( "Список блоков провайдера данных не может быть равным null.");
            }

            foreach (var block in blocks)
            {
                var b = new Block(block.Hash, block.PreviousHash, block.TimeStamp, block.Index, block.MerkleRoot, block.CurrentTransactions );
                BlockChain.Add(b);

            } 
        }

        public Chain(Block block)
        {

            if (block == null)
            {
                throw new InvalidBlockException("Блок провайдера данных не может быть равным null.");
            }

            var b = new Block(block.Hash, block.PreviousHash, block.TimeStamp, block.Index, block.MerkleRoot, block.CurrentTransactions);
            BlockChain.Add(b);
        }

        public Chain()
        {
           
        }

      
        /// <summary>
        /// Проверить корректность цепочки блоков.
        /// </summary>
        /// <returns> Корректность цепочки блоков. true - цепочка блоков корректна, false - цепочка некорректна. </returns>
        public bool CheckCorrect(Block newBlock)
        {

            Chain chain = new Chain( BlockSubscribe.GetBlocks().ToList());

            if (chain.BlockChain.Any(b => b.Hash == newBlock.Hash))
            {
                return false;
            }

            chain.BlockChain.Add(newBlock);
            
            for (int i = 0; i <= chain.BlockChain.Count; i += 2)
            {
                if (i + 1 < chain.BlockChain.Count)
                {
                    if(!chain.BlockChain[i + 1].PreviousHash.Equals(chain.BlockChain[i].Hash))
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
        protected void LoadDataFromLocalChain(Chain localChain)
        {
            if (localChain == null)
            {
                throw new InvalidBlockException("Локальная цепочка блоков не может быть равна null.");
            }

            foreach (var block in localChain.blockChain)
            {
                BlockChain.Add(block);
                

            }
        }

    }
}
