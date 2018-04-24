using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataChain.DataLayer;
using System.Security.Cryptography;

namespace DataChain.Infrastructures
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

            for (int i = 0; i <= countTransactions; i += 2)
                {

                level++;
                if (i + 1 < countTransactions)
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

            if (countTransactions % 2 == 1)
            {

                merkleList.Insert(merkleList.Count ,
                    new HexString(ComputeHashData(data.CurrentTransactions[countTransactions - 1].Hash,
                                                   data.CurrentTransactions[countTransactions - 1].Hash)));
                countTransactions += 1;
            }
            if ((countTransactions /2) != 1)
            {
                GetMerkleRoot(data, countTransactions / 2);
            }
            var root = merkleList[merkleList.Count - 1];

            return root;
        }
        
    }
}
