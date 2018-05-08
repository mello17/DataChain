using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;
using System.Data.Entity;

namespace DataChain.EntityFramework
{
   public class BlockSubscriber : IBlockSubscriber
    {
        private DatachainContext db = new DatachainContext();


        public void Init()
        {
            Database.SetInitializer(new MigrationInitializer());
        }
        public async Task<Block> GetBlock(uint id)
        {
            var block = await db.Blocks.FindAsync(id);
            
            if (block == null)
            {
                throw new InvalidBlockException($"Block with id  {id} can not find");
            }

            var response =  Serializer.DeserializeBlock(block);

            return await Task.FromResult(response);
        }

        public async  Task<Block> GetBlock(HexString hash)
        {
            var block =   db.Blocks.Where(b=> b.BlockHash == hash.ToByteArray()).SingleOrDefault();

            if (block == null)
            {
                throw new InvalidBlockException($"Block with hash  {hash} can not find");
            }

            var response = Serializer.DeserializeBlock(block);

            return await Task.FromResult(response);
        }

        public async Task<Block> GetLatestBlock()
        {
            var last = db.Blocks.Max(b => b.Id);

            if(last == 0)
            {
                throw new InvalidBlockException("Chain is empty");
            }

            return await GetBlock((uint)last);

        }
    }
}
