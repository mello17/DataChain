using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;
using System.Security.Cryptography;
using DataChain.DataLayer;

namespace DataChain.Infrastructures
{
    public class ChainSerializer
    {

        public ChainSerializer()
        {
           // localChain = _localChain;
        }

        public byte[] Encode(IEnumerable<Block> chain)
        {
           
            MemoryStream stream = new MemoryStream();

            foreach (var block in chain)
            {

                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(BitConverter.GetBytes(block.Index), 0, BitConverter.GetBytes(block.Index).Length);
                stream.Write(block.Hash.ToByteArray(), 0, block.Hash.ToByteArray().Length);
                stream.Write(block.MerkleRoot.ToByteArray(), 0, block.MerkleRoot.ToByteArray().Length);
                stream.Write(block.PreviousHash.ToByteArray(), 0, block.PreviousHash.ToByteArray().Length);
                stream.Write(BitConverter.GetBytes(block.TimeStamp.ToBinary()), 0, BitConverter.GetBytes(block.TimeStamp.ToBinary()).Length);
            }
            ECKeyValidator key = new ECKeyValidator();
            var encrypt = key.RSA.Encrypt(stream.ToArray(), false);
            return encrypt;
        }

       

        public IEnumerable<Block> Decode(byte[] rawChain)
        {
            throw new NotImplementedException();
        }



    }
}
