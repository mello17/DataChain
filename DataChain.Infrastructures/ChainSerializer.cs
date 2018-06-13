using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DataChain.Abstractions;
using DataChain.DataProvider;
using DataChain.Abstractions.Interfaces;

namespace DataChain.Infrastructure
{
    public class ChainSerializer
    {
        private readonly ECKeyValidator key;
       
        private ITransactionRepository subscribe;
        public byte[] BigEndianData { get; private set; }

        public ChainSerializer()
        {
            key = new ECKeyValidator();
            subscribe = new TransactionRepository();
           
        }

        public Tuple<byte[],byte[]> Encode(IEnumerable<Block> chain)
        {

             byte[] bigEndianData;
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
                    writer.Write(block.CurrentTransactions.Count);

                    foreach (var tx in block.CurrentTransactions)
                    {
                        writer.Write(tx.Hash.ToString());
                       
                    }

                    writer.Flush();
                }
                bigEndianData = Aes256.Encode(ToBigEndian(stream.ToArray()));
            }
          
           
            var encrypt = key.RSA.Encrypt(Aes256.GetBytes(Aes256.RandomKey), false);
            
            return new Tuple<byte[],byte[]>(encrypt, bigEndianData);
        }
       

        public IEnumerable<Block> Decode(byte[] data)
        {

            byte[] encryptData = new byte[256];
            byte[] bigEndianData = null;
            using (MemoryStream rawStream = new MemoryStream(data))
            {
                using(BinaryReader reader= new BinaryReader(rawStream))
                {
                    encryptData =  reader.ReadBytes(256);
                    bigEndianData = reader.ReadBytes((int)reader.BaseStream.Length - encryptData.Length);
                }
            }
            
            var decryptKey = key.RSA.Decrypt(encryptData, false);
            var decrypt = Aes256.Decode(bigEndianData, Aes256.GetString(decryptKey));
           
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
                        blockKeyValuePairs["count"] = reader.ReadInt32();

                        List<Transaction> list = new List<Transaction>();
                        for (int i = 0; i < (int)blockKeyValuePairs["count"]; i++)
                        {
                            blockKeyValuePairs["tx_hash"] = reader.ReadString();
                            var tx = subscribe.GetTransactionAsync(
                                HexString.Parse((string)blockKeyValuePairs["tx_hash"])
                                .ToByteArray()).Result;
                            list.Add(tx);
                        }
                       

                        blocks.Add(new Block(HexString.Parse((string)blockKeyValuePairs["hash"]),
                                           HexString.Parse((string)blockKeyValuePairs["previousHash"]),
                                           DateTime.FromBinary((long)blockKeyValuePairs["stamp"]),
                                           (int)blockKeyValuePairs["index"],
                                           HexString.Parse((string)blockKeyValuePairs["merkleroot"]),
                                           list
                                          ));
                       

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

        public byte[] ConcateByteArray(Tuple<byte[],byte[]> byteTuple)
        {

            using (MemoryStream stream = new MemoryStream())
            {

                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(byteTuple.Item1);
                    writer.Write(byteTuple.Item2);
                }


                return stream.ToArray();
            }
        }

    }
}
