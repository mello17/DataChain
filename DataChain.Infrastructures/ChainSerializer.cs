using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using DataChain.Abstractions;
using DataChain.DataProvider;


namespace DataChain.Infrastructure
{
    public class ChainSerializer
    {
        private readonly ECKeyValidator key;
        public byte[] BigEndianData {get; private set;}

        public ChainSerializer()
        {
            key = new ECKeyValidator();
           
        }

        public byte[] Encode(IEnumerable<Block> chain)
        {
           
            MemoryStream stream = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);

            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach (var block in chain)
                {
                    writer.Write(block.Index);
                    writer.Write(block.Hash.ToString());
                    writer.Write(block.MerkleRoot.ToString());
                    writer.Write(block.PreviousHash.ToString());
                    writer.Write(block.TimeStamp.ToBinary());
                    writer.Flush();
                }
            }
          
            this.BigEndianData = Aes256.Encode(ToBigEndian(stream.ToArray()));
            var encrypt = key.RSA.Encrypt(Aes256.GetBytes(Aes256.RandomKey), false);
            return encrypt;
        }
       

        public IEnumerable<Block> Decode(byte[] data)
        {

            var decryptKey = key.RSA.Decrypt(data, false);
            var decrypt = Aes256.Decode(this.BigEndianData, Aes256.GetString(decryptKey));
           
            MemoryStream stream = new MemoryStream(decrypt);
            List<Block> blocks = new List<Block>();

            Dictionary<string, object> blockKeyValuePairs = new Dictionary<string, object>();

            try
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (reader.BaseStream.Position <= reader.BaseStream.Length)
                    {
                        blockKeyValuePairs["index"] = reader.ReadInt32();
                        blockKeyValuePairs["hash"] = reader.ReadString();
                        blockKeyValuePairs["merkleroot"] = reader.ReadString();
                        blockKeyValuePairs["previousHash"] = reader.ReadString();
                        blockKeyValuePairs["stamp"] = reader.ReadInt64();

                        blocks.Add(new Block(HexString.Parse((string)blockKeyValuePairs["hash"]),
                                           HexString.Parse((string)blockKeyValuePairs["previousHash"]),
                                           DateTime.FromBinary((long)blockKeyValuePairs["stamp"]),
                                           (int)blockKeyValuePairs["index"],
                                           HexString.Parse((string)blockKeyValuePairs["merkleroot"]),
                                           new BlockMetadata()));

                    }
                }
            }
            catch (EndOfStreamException)
            {
                
            }
          

            return blocks.AsEnumerable();
        }


        private byte[] ToBigEndian(byte[] value)
        {

            var temp = value;

            if(temp.Length > 0 && temp[temp.Length - 1] == 0)
            {
                Array.Resize(ref temp, temp.Length - 1);
                Array.Reverse(temp);

            }

            return temp;
        }

    }
}
