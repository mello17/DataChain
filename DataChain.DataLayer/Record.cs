using System;


namespace DataChain.DataLayer
{
   public class Record
    {
        public int Version { get; set; }
        public string Name { get; set; }
        public HexString Value { get; set; }

    }
}
