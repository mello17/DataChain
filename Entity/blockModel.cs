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
        public byte[] BlockHash { get; set; }

        [ConcurrencyCheck]
        [Required]
        public byte[] PreviousHash { get; set; }

        [ConcurrencyCheck]
        [Required]
        public string Timestamp { get; set; }

        public virtual ICollection<TransactionModel> Transactions { get; set; }

        public BlockModel()
        {
            Transactions = new List<TransactionModel>();
        }
    }
}
