using System;


namespace DataChain.Abstractions
{
   public class Record
    {
        public int Version { get; set; }
        public string Name { get; set; }
        public HexString Value { get; set; }
        public TypeData TypeRecord { get; set; }

        public Record(int _version, string _name, HexString _value, TypeData _type)
        {

            this.Version = _version;
            this.Name = _name ?? throw new ArgumentNullException(nameof(_name));
            this.Value = _value;
            this.TypeRecord = _type;
        }

    }
}
