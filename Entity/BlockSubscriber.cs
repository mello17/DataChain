using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.Infrastructures;
using DataChain.DataLayer;
using DataChain.DataLayer.Interfaces;

namespace DataChain.EntityFramework
{
   public class BlockSubscriber : IBlockSubscriber
    {
        private DatachainContext db = new DatachainContext();

        public async Task<Block> GetBlock(uint id)
        {
            var block = await db.Blocks.FindAsync(id);

            if (block == null)
            {
                throw new InvalidBlockException($"Block with id  {id} can not find");
            }

            var response =  Serializer.DeserializeBlock(block);

            return response;
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
