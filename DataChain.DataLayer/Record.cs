using System;


namespace DataChain.DataLayer
{
   public class Record
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public HexString Value { get; set; }
        public HexString Key { get; set; }



    }
}
