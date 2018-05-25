using System;
using DataChain.DataLayer;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DataChain.DataLayer.Interfaces;
using DataChain.EntityFramework;
using NLog;
using System.Net.WebSockets;
using System.IO;
using System.Threading;

namespace DataChain.Infrastructures
{
    public class BlockBuilder
    {
       private IBlockSubscriber subscribe = new BlockSubscriber();
       private ITransactionSubscriber txSubscriber = new TransactionSubscriber();
       private Logger log = LogManager.GetCurrentClassLogger();

       public BlockBuilder()
       {

            this.LatestBlock = subscribe.GetLatestBlock();
       }

        public Block LatestBlock
        {
            get; private set;
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
            var metaData = ComputeMetadata(tx);
            
            var merkleroot = MerkleTree.GetMerkleRoot(metaData, metaData.TransactionCount);
            var timestamp = DateTime.UtcNow;

            var nextHash = ComputeBlockHash(new Block(prevHash, prevHash, timestamp, nextIndex, merkleroot, metaData));

            return new Block( nextHash, prevHash, timestamp,nextIndex, merkleroot, metaData );
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
                using (MemoryStream stream = new MemoryStream())
                {
                    await socket.SendAsync(segment, WebSocketMessageType.Binary, true, CancellationToken.None);
                }

            }

        }

        public async Task CompleteBlockAdding()
        {
            try
            {
               var tx_list =  txSubscriber.GetLastTransactionAsync();
               var newBlock = GenerateBlock(tx_list);
               AddBlock(newBlock);
               await CommitBlock(newBlock);

            }
            catch (Exception ex)
            {
                log.Error("Error when respond block " + ex.Message);
                throw new InvalidBlockException("Error when respond block " + ex.Message);
            }
        }

        private BlockMetadata ComputeMetadata(List<Transaction> tx)
        {
            var count = tx.Count;
            return new BlockMetadata() {
                CurrentTransactions = tx,
                Instance = 1,
                TransactionCount = count
                };
        }

        private HexString ComputeBlockHash( Block previousBlock)
        {

            var header = new BlockHeader().ComputeBlockHeader(previousBlock.Index,
                previousBlock.PreviousHash, 
                previousBlock.TimeStamp,
                previousBlock.MerkleRoot);
            return new HexString( Serializer.ComputeHash( Serializer.ToBinaryArray(header)));
        }

        public bool IsValidNewBlock(Block newBlock, Block previousBlock)
        {
            

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
