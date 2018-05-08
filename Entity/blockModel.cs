using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DataChain.EntityFramework
{
   public class BlockModel
    {
        [Key]
        public int Id { get; private set; }

        [ConcurrencyCheck]
        [Required]
        [Index]
        [MaxLength(1024)]
        public byte[] BlockHash { get; private set; }

        [ConcurrencyCheck]
        [Required]
        public byte[] PreviousHash { get; private set; }

        [ConcurrencyCheck]
        [Required]
        public DateTime Timestamp { get; private set; }

        public virtual ICollection<TransactionModel> Transactions { get; set; }

        public BlockModel()
        {
            Transactions = new List<TransactionModel>();
        }
    }
}
