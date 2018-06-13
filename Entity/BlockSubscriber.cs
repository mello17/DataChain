using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.Abstractions;
using DataChain.Abstractions.Interfaces;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace DataChain.DataProvider
{
   public class BlockRepository : IBlockRepository
    {
        private readonly DatachainContext database;

        public BlockRepository()
        {
            database = new DatachainContext();
        }

        public void Init()
        {
            Database.SetInitializer(new MigrationInitializer());
        }

        public async Task<Block> GetBlock(int id)
        {
            var block = await database.Blocks.FindAsync(id);
            
            if (block == null)
            {
                throw new InvalidBlockException($"Block with id  {id} can not find");
            }

            var response =  Serializer.DeserializeBlock(block);

            return await Task.FromResult(response);
        }

        

        public List<Block> GetBlocks()
        {
            var list = new List<Block>();
            foreach(var block in database.Blocks)
            {
                list.Add(Serializer.DeserializeBlock(block));
            }

            return list;
        }

        public async  Task<Block> GetBlock(HexString hash)
        {
            var block = database.Blocks.Where(b=> b.BlockHash == hash.ToByteArray()).SingleOrDefault();

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
                block = database.Blocks.OrderByDescending(b=> b.Id).FirstOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception(ex.Message);
            }

            if(block == null)
            {
                return null;
            }

            return Serializer.DeserializeBlock(block);

        }

        public void Clear()
        {
            database.Blocks.RemoveRange(database.Blocks);
            database.SaveChanges();
        }

        public void AddBlock(Block block)
        {
           
            var  model = Serializer.SerializeBlock(block);

            try
            {
                database.Blocks.Add(model);
                database.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                throw new InvalidOperationException(ex.EntityValidationErrors.First()
                    .ValidationErrors
                    .FirstOrDefault()
                    .ErrorMessage);
            }
        }
    }
}
