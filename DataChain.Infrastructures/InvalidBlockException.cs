using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataChain.Infrastructures
{

    public  class InvalidBlockException : Exception
    {
        public string Reason { get; set; }

        public InvalidBlockException(string _reason) : base(String.Format($"Block cannot anchoring :  {_reason}"))
        {
            this.Reason = _reason;
        }

    }
}
