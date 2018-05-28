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

        protected List<Block> _blockChain = new List<Block>();
        private List<string> _hosts = new List<string>();
        private List<Account> _users = new List<Account>();

        public IEnumerable<Block> BlockChain => _blockChain;

        public Block PreviousBlock => _blockChain.Last();

        public IEnumerable<Account> Users => _users;
        public IEnumerable<string> Hosts => _hosts;
        public int Length => _blockChain.Count;
        public IBlockSubscriber BlockSubscribe { get; private set; } = new BlockSubscriber();
        public ITransactionSubscriber TransactionSubscribe { get; private set; } = new TransactionSubscriber();



        public Chain(List<Block> blocks)
        {
            if (blocks == null)
            {
                throw new InvalidBlockException( "Список блоков провайдера данных не может быть равным null.");
            }

            foreach (var block in blocks)
            {
                var b = new Block(block.Hash, block.PreviousHash, block.TimeStamp, block.Index, block.MerkleRoot, block.Metadata );
                _blockChain.Add(b);

              //  AddDataInList(b);
            }

            
        }

        public Chain(Block block)
        {

            if (block == null)
            {
                throw new InvalidBlockException("Блок провайдера данных не может быть равным null.");
            }

            var b = new Block(block.Hash, block.PreviousHash, block.TimeStamp, block.Index, block.MerkleRoot, block.Metadata);
            _blockChain.Add(b);

           
        }

        /// <summary>
        /// Создать новый экземпляр цепочки блоков.
        /// </summary>
        public Chain()
        {
           
        }
      

        public void AddBlock(Block block)
        {
           
            if (_blockChain.Any(b => b.Hash == block.Hash))
            {
                return;
            }

            _blockChain.Add(block);
            BlockSubscribe.AddBlock(block);
          
            
            SendBlockToGlobalChain(block);

           
        }

      
        /// <summary>
        /// Проверить корректность цепочки блоков.
        /// </summary>
        /// <returns> Корректность цепочки блоков. true - цепочка блоков корректна, false - цепочка некорректна. </returns>
        public bool CheckCorrect()
        {
            for (int i = 0; i <= _blockChain.Count; i += 2)
            {
                if (i + 1 < _blockChain.Count)
                {
                    if(_blockChain[i].PreviousHash != _blockChain[i + 1].Hash)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
       
       

      

        /// <summary>
        /// Добавление данных из блоков в списки быстрого доступа.
        /// </summary>
        /// <param name="block"> Блок. </param>
        public void SendBlockToGlobalChain(Block block)
        {
            
            throw new NotImplementedException();
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

            foreach (var block in localChain._blockChain)
            {
                _blockChain.Add(block);
                

            }
        }

        /// <summary>
        /// Отправка запроса к api хоста.
        /// </summary>
        /// <param name="ip"> Адрес хоста сети. </param>
        /// <param name="method"> Метод вызываемый у хоста. </param>
        /// <param name="data"> Передаваемые параметры метода через &. </param>
        /// <returns> Json ответ хоста. </returns>
        public static string SendRequest(string ip, string method, string data)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(20);

                // http://localhost:28451/api/Main/getchain/ пример запроса.
                string repUri = $"{ip}/api/Main/{method}/{data}";
                var response = client.GetAsync(repUri).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    return result;
                }
            }

            return null;
        }

        

        /// <summary>
        /// Запрос к api хоста на добавление блока данных.
        /// </summary>
        /// <param name="ip"> Адрес хоста в сети. </param>
        /// <param name="method"> Вызываемый метод хоста. </param>
        /// <param name="data"> Параметры метода хоста через &.</param>
        /// <returns> Успешность выполнения запроса. </returns>
        public bool SendBlockToHosts(string ip, string method, string data)
        {
            var result = SendRequest(ip, method, data);
            var success = !string.IsNullOrEmpty(result);
            return success;
        }
    }
}
