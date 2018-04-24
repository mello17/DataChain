using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DataChain.EntityFramework
{
   [Table("Transactions")]
   public class TransactionModel
    {
        [Key]
        public int Id { get; set; }

        [ConcurrencyCheck]
        [Required]
        [Index]
        public byte[] TransactionHash { get; set; }

        [ConcurrencyCheck]
        [Required]
        public string Timestamp { get; set; }

        [ConcurrencyCheck]
        [Required]
        public byte[] RawData { get; set; }

        public virtual ICollection<RecordModel> Records { get; set; }

        public TransactionModel()
        {
            Records = new List<RecordModel>();
        }

    }
}
