using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.DataLayer
{
    public  class User
    {
        
        public string Login { get;  }
        public HexString Password { get; }
        public IReadOnlyList<Block> CreatedBlocks { get; }
    }
}
