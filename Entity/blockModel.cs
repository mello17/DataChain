using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DataChain.EntityFramework
{
   public class BlockModel
    {
        [Key]
        public int Id { get; set; }

        [ConcurrencyCheck]
        [Required]
        [Index]
        [MaxLength(900)]
        public byte[] BlockHash { get; set; }

        [ConcurrencyCheck]
        [Required]
        public byte[] PreviousHash { get; set; }

        [ConcurrencyCheck]
        [Required]
        public DateTime Timestamp { get; set; }

        [ConcurrencyCheck]
        [Required]
        [Index]
        [MaxLength(900)]
        public byte[] MerkleRoot { get; set; }

        public int? AccountId { get; set; }
        public virtual AccountModel AccountModel { get; set; }

        public virtual ICollection<TransactionModel> Transactions { get; set; }

        public BlockModel()
        {
            Transactions = new List<TransactionModel>();
        }
    }
}
