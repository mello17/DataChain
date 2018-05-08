﻿using System;
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
        [MaxLength(1024)]
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
        [MaxLength(1024)]
        public byte[] Signature { get; set; }

        public virtual int?  BlockModelId { get; set; }
        public virtual BlockModel BlockModel { get; set; }

        public virtual ICollection<RecordModel> Records { get; set; }

        public TransactionModel()
        {
            Records = new List<RecordModel>();
        }

    }
}
