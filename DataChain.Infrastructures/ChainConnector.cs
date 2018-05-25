using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.DataLayer;
using DataChain.EntityFramework;

namespace DataChain.Infrastructures
{
    public  class ChainConnector : Chain
    {

        public ChainConnector() : base()
        {
           // ChainReplace();
        }

        public Chain GetGlobalChain()
        {

            foreach (var host in Hosts)
            {
                // TODO: Здесь нужно будет переделать. Предварительно выбирается хост с самой большой цепочкой блоков и уже он синхранизуется.
                var blocks = GetBlocksFromHosts(host);
                if (blocks != null && blocks.Count > 0)
                {
                    return new Chain(blocks);
                }
            }

            return null;
        }


        /// <summary>
        /// Получение всех блоков от хоста через api.
        /// </summary>
        /// <param name="ip"> Адрес хоста в сети. </param>
        /// <returns> Список блоков. </returns>
        private static List<Block> GetBlocksFromHosts(string ip)
        {
            // http://localhost:28451/api/getchain/ пример запроса.
            var response = SendRequest(ip, "getchain", "");
            if (string.IsNullOrEmpty(response))
            {
                return null;
            }
            else
            {
                // var blocks = DeserializeCollectionBlocks(response);
                return null;
            }
        }

        private void CreateNewBlockChain()
        {
            BlockSubscribe.Clear();
            _blockChain = new List<Block>();
            var genesisBlock = Genesis.CreateGenesis();
            AddBlock(genesisBlock);
        }


        public void ChainReplace()
        {
           
            // Получаем цепочки блоков.
            var globalChain = GetGlobalChain();
            var localChain = GetLocalChain();

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

        /// <summary>
        /// Получить данные из локальной цепочки.
        /// </summary>
        /// <param name="localChain"> Локальная цепочка блоков. </param>
        private void LoadDataFromLocalChain(Chain localChain)
        {
            if (localChain == null)
            {
                throw new InvalidBlockException("Локальная цепочка блоков не может быть равна null.");
            }

            foreach (var block in _blockChain)
            {
                _blockChain.Add(block);
                AddDataInList(block);
                
            }
        }

        private void ReplaceLocalChainFromGlobalChain(Chain localChain, Chain globalChain)
        {
            if (globalChain == null || localChain == null)
            {
                throw new InvalidBlockException("Глобальная цепочка блоков не может быть равна null.");
            }

            var countNewBlocks = globalChain.Length - localChain.Length;
           
            //   blockSubscriber.Clear();

            var newBlocks = globalChain.BlockChain.Reverse().Take(countNewBlocks).ToList();
            foreach (var block in newBlocks)
            {
                AddBlock(block);
            }
        }


    }
}
