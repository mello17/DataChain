using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataChain.DataLayer;


namespace DataChain.EntityFramework
{

    [Table("Records")]
    public class RecordModel
    {
        [Key]
        public int Id { get; set; }

        [ConcurrencyCheck]
        public byte[] Value { get; set; }

        [ConcurrencyCheck]
        [Required]
        [Index]
        [MaxLength(512)]
        public string Name { get; set; }

        [Column(TypeName ="int")]
        [Required]
        public TypeData Type { get; set; }

        public int? TransactionId { get; set; }
        public virtual TransactionModel TransactionModel { get; set; }

    }
}
