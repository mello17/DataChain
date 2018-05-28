using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DataChain.DataProvider
{
   [Table("Transactions")]
   public class TransactionModel
    {
        [Key]
        public int Id { get; set; }

        [ConcurrencyCheck]
        [Required]
        [Index]
        [MaxLength(512)]
        public byte[] TransactionHash { get; set; }

        [ConcurrencyCheck] 
        [Required]
        public DateTime Timestamp { get; set; }

        [ConcurrencyCheck]
        [Required]
        public byte[] RawData { get; set; }

        [ConcurrencyCheck]
        [Required]
        [Index]
        [MaxLength(750)]
        public byte[] Signature { get; set; }

        [ConcurrencyCheck]
        [Required]
        [Index]
        [MaxLength(450)]
        public byte[] PubKey { get; set; }

        public virtual int?  BlockModelId { get; set; }
        public virtual BlockModel BlockModel { get; set; }

        public virtual ICollection<RecordModel> Records { get; set; }

        public TransactionModel()
        {
            Records = new List<RecordModel>();
        }

    }
}
