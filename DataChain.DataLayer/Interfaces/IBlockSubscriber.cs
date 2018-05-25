using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.DataLayer.Interfaces
{
    public interface IBlockSubscriber
    {
        Task<Block> GetBlock(int id);
        Task<Block> GetBlock(HexString hash);
        Block GetLatestBlock();
        IEnumerable<Block> GetBlocks();
        void Init();
        void Clear();
        void AddBlock(Block block);
    }
}
