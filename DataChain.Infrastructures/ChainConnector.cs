using System;
using System.Collections.Generic;
using System.Linq;
using DataChain.DataProvider;

namespace DataChain.Infrastructure
{
    public  class ChainConnector : Chain
    {

        public ChainConnector() : base()
        {
           
        }
       

        public void CreateNewBlockChain()
        {

            BlockSubscribe.Clear(); 
        }


        public void ChainReplace(Chain localChain, Chain globalChain)
        {

            if (globalChain != null && localChain != null)
            {
                if (globalChain.Length > localChain.Length)
                {
                    ReplaceLocalChainFromGlobalChain(localChain, globalChain);
                }
                else
                {
                    LoadDataFromLocalChain(localChain);
                }
            }
            else if (globalChain != null)
            {
                ReplaceLocalChainFromGlobalChain(localChain, globalChain);
            }
            else if (localChain != null)
            {
                LoadDataFromLocalChain(localChain);
            }
            else
            {
                CreateNewBlockChain();
            }
        }

        public Chain GetLocalChain()
        {
            var blocks = BlockSubscribe.GetBlocks().ToList();
            if (blocks.Count() > 0)
            {
                return new Chain(blocks);
            }

            return null;
        }

        private void ReplaceLocalChainFromGlobalChain(Chain localChain, Chain globalChain)
        {
            if (globalChain == null || localChain == null)
            {
                throw new InvalidBlockException("Глобальная цепочка блоков не может быть равна null.");
            }

            var countNewBlocks = globalChain.Length - localChain.Length;
           
            //   blockSubscriber.Clear();

            var newBlocks = globalChain.BlockChain.AsEnumerable().Reverse().Take(countNewBlocks).ToList();
            foreach (var block in newBlocks)
            {
                BlockChain.Add(block);
            }
        }


    }
}
