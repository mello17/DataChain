using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DataChain.EntityFramework
{

    [Table("Records")]
    public class RecordModel
    {
        [Key]
        public int Id { get; set; }

        [ConcurrencyCheck]
        [Required]
        [Index]
        public int Instance { get; set; }

        [ConcurrencyCheck]
        [Required]
        public byte[] Value { get; set; }

        [ConcurrencyCheck]
        [Required]
        public string Name { get; set; }

        public int? TransactionId { get; set; }
        public virtual TransactionModel TransactionModel { get; set; }

    }
}
