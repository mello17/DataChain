using System;
using System.Collections.Generic;
using DataChain.Abstractions;
using System.Security.Cryptography;

namespace DataChain.Infrastructure
{
    public static class MerkleTree
    {
        static IList<HexString> merkleList = new List<HexString>();
        static int level = 0;

        private static byte[] ComputeHashData(HexString left, HexString right)
        {
        
            SHA256 sha =  SHA256.Create();
            return sha.ComputeHash(sha.ComputeHash(left + right));
        }

      
        public static HexString GetMerkleRoot( BlockMetadata data, int countTransactions)
        {
            var tempCount = countTransactions;

            for (int i = 0; i <= tempCount; i += 2)
                {

                level++;
                if (i + 1 < tempCount)
                {
                    if (level == 1 )
                    {
                        merkleList.Add(new HexString(ComputeHashData(data.CurrentTransactions[i].Hash,
                                                    data.CurrentTransactions[i + 1].Hash)));
                        level = 0;
                    }
                    else 
                    {
                        merkleList.Add(new HexString(ComputeHashData(merkleList[i], merkleList[i + 1])));
                    }
                }
               
                }

            if (tempCount % 2 == 1)
            {

                merkleList.Insert(merkleList.Count ,
                    new HexString(ComputeHashData(data.CurrentTransactions[countTransactions - 1].Hash,
                                                   data.CurrentTransactions[countTransactions - 1].Hash)));
                tempCount += 1;
            }
            if ((tempCount / 2) != 1)
            {
                GetMerkleRoot(data, tempCount / 2);
            }
            var root = merkleList[merkleList.Count - 1];

            return root;
        }
        
    }
}
