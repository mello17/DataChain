using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.Infrastructure
{
    public class HexString :IEquatable<HexString>
    {
        private readonly byte[] data;

        public IReadOnlyList<byte> Value { get; }
        public HexString Empty { get; }

        public  bool Equals(HexString obj)
        {
            if(obj == null)
            {
                return false;
            }

            else
            {
                if (this.data.Length != obj.data.Length)
                    return false;

                for (int i = 0; i < obj.data.Length; i++)
                    if (this.data[i] != obj.data[i])
                        return false;

                return true;
            }
        }
    }
}
