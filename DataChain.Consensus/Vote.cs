using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.Consensus
{
    public class Vote
    {
        public const bool VALUE_APPROVE = true;
        public const bool VALUE_REJECT = false;

        public long Height { get; set; }
        public int View { get; set; }
        public EventType Type { get; set; }
        public byte[] BlockHash { get; set; }
        public bool Value { get; set; }
        public bool Validated { get; set; }

        public Vote(EventType _type, bool _value, long _height, int _view, byte[] _blockHash, bool _validated)
        {
            this.Type = _type;
            this.Value = _value;
            this.Height = _height;
            this.View = _view;
            this.BlockHash = _blockHash;
            this.Validated = _validated;

        }
    }
}
