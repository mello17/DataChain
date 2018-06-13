using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.Abstractions.Interfaces
{
    public interface IBlockRepository
    {
        Task<Block> GetBlock(int id);
        Task<Block> GetBlock(HexString hash);
        Block GetLatestBlock();
        List<Block> GetBlocks();
        void Init();
        void Clear();
        void AddBlock(Block block);
        
    }
}
