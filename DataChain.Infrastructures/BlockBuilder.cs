using System;
using DataChain.Abstractions;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DataChain.Abstractions.Interfaces;
using DataChain.DataProvider;
using NLog;
using System.Net.WebSockets;
using System.IO;
using System.Threading;

namespace DataChain.Infrastructure
{
    public class BlockBuilder
    {
       private IBlockSubscriber subscribe;
       private ITransactionSubscriber txSubscriber;
       private Logger log;
       private ChainConnector connector;

       public BlockBuilder(IBlockSubscriber _subscribe, ITransactionSubscriber _txSubscriber)
       {

            subscribe = _subscribe;
            txSubscriber = _txSubscriber;
            connector = new ChainConnector();   
            log = LogManager.GetCurrentClassLogger();
            this.LatestBlock = subscribe.GetLatestBlock();
            if (this.LatestBlock == null)
            {
                connector.CreateNewBlockChain();
            }
       }

        public Block LatestBlock
        {
            get; private set;
        }

        private string ComputeBlockHeader(Block block)
        {
            return string.Concat(block.Index, block.PreviousHash, block.TimeStamp, block.MerkleRoot);
        }

        public Block GenerateBlock( List<Transaction> tx)
        {
            if (tx.Count == 0)
            {
                return null;
            }

            if (this.LatestBlock == null)
            {
                var genesis = Genesis.CreateGenesis();
                AddBlock(genesis);
                log.Info("Create genesis block");
                this.LatestBlock = genesis;
            }

            var prevHash = this.LatestBlock.Hash;
            var nextIndex = this.LatestBlock.Index + 1;
            var metaData = Serializer.ComputeMetadata(tx);
            
            var merkleroot = MerkleTree.GetMerkleRoot(metaData, metaData.TransactionCount);
            var timestamp = DateTime.UtcNow;

            var nextHash = ComputeBlockHash(new Block(prevHash, prevHash, timestamp, nextIndex, merkleroot));

            return new Block( nextHash, prevHash, timestamp,nextIndex, merkleroot );
        }

        public void AddBlock(Block newBlock)
        {
            if (newBlock == null)
            {
                throw new ArgumentNullException(nameof(newBlock));
            }

            if (!IsValidNewBlock(newBlock, this.LatestBlock))
            {
                throw new InvalidBlockException("Invalid new block");
            }

            subscribe.AddBlock(newBlock);


        }

        public async Task CommitBlock(Block newBlock)
        {
            byte[] buffer = new byte[1024 * 1024];
            ChainSerializer chainSerializer = new ChainSerializer();
            buffer = chainSerializer.Encode(new[] { newBlock });
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer);

            using (ClientWebSocket socket = new ClientWebSocket())
            {

                UriBuilder uri = new UriBuilder("ws://localhost:16797/");

                WebSocketReceiveResult receiveResult = await socket.ReceiveAsync(
                       segment, CancellationToken.None);

                await socket.ConnectAsync(uri.Uri, CancellationToken.None);
                await socket.SendAsync(segment, WebSocketMessageType.Binary, true, CancellationToken.None);
                
            }

        }

        public async Task CompleteBlockAdding(CancellationToken token)
        {
            try
            {

               var tx_list =  txSubscriber.GetLastTransactionAsync();
               Block newBlock = GenerateBlock(tx_list);

               if (newBlock == null) return;
               
               AddBlock(newBlock);
               await CommitBlock(newBlock);

              // await Task.Delay(TimeSpan.FromMinutes(5), token);

            }
            catch (Exception ex)
            {
                log.Error("Error when respond block " + ex.Message);

                await Task.Delay(TimeSpan.FromMinutes(1), token);   
            }
        }

       

        private HexString ComputeBlockHash( Block previousBlock)
        {

            var header = ComputeBlockHeader(previousBlock);
            return new HexString( Serializer.ComputeHash( Serializer.ToBinaryArray(header)));
        }

        public bool IsValidNewBlock(Block newBlock, Block previousBlock)
        {

            Chain chain = new Chain(newBlock);
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
            else if (previousBlock.TimeStamp > newBlock.TimeStamp)
            {
                log.Error("Invalid timestamp. New block cannot create in future");
                return false;
            }
           
            else if (!connector.CheckCorrect())
            {
                log.Error("Invalid block");
                return false;
            }

            return true;
        }
        
       


    }
}
