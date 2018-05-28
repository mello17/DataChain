using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.Abstractions;
using DataChain.Abstractions.Interfaces;
using System.Data.Entity;

namespace DataChain.DataProvider
{
   public class BlockSubscriber : IBlockSubscriber
    {
        private DatachainContext db = new DatachainContext();


        public void Init()
        {
            Database.SetInitializer(new MigrationInitializer());
        }

        public async Task<Block> GetBlock(int id)
        {
            var block = await db.Blocks.FindAsync(id);
            
            if (block == null)
            {
                throw new InvalidBlockException($"Block with id  {id} can not find");
            }

            var response =  Serializer.DeserializeBlock(block);

            return await Task.FromResult(response);
        }

        public IEnumerable<Block> GetBlocks()
        {
            var list = new List<Block>();
            foreach(var block in db.Blocks.ToList())
            {
                list.Add(Serializer.DeserializeBlock(block));
            }

            return list;
        }

        public async  Task<Block> GetBlock(HexString hash)
        {
            var block = db.Blocks.Where(b=> b.BlockHash == hash.ToByteArray()).SingleOrDefault();

            if (block == null)
            {
                throw new InvalidBlockException($"Block with hash  {hash} can not find");
            }

            var response = Serializer.DeserializeBlock(block);

            return await Task.FromResult(response);
        }

        public Block GetLatestBlock()
        {

            BlockModel block;
            try
            {
                block = db.Blocks.OrderByDescending(b=> b.Id).FirstOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception(ex.Message);
            }


            return Serializer.DeserializeBlock(block);

        }

        public void Clear()
        {
            db.Blocks.RemoveRange(db.Blocks);
            db.SaveChanges();
        }

        public void AddBlock(Block block)
        {
           
            var  model = Serializer.SerializeBlock(block);

            db.Blocks.Add(model);
            db.SaveChanges();
        }
    }
}
