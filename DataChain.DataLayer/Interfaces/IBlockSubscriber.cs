using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.DataLayer.Interfaces
{
    public interface IBlockSubscriber
    {
        Task<Block> GetBlock(uint id);
        Task<Block> GetBlock(HexString hash);
        Task<Block> GetLatestBlock();
        void Init();
    }
}
