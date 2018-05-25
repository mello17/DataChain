using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.DataLayer
{
    public class BlockHeader 
    {

        public string ComputeBlockHeader(int _index, HexString _prevHash, DateTime _timeStamp, HexString _merkle)
        {
            return string.Concat(_index, _prevHash, _timeStamp, _merkle);
        }
    }
}
