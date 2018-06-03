using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using DataChain.Abstractions;

namespace DataChain.DataProvider
{
    public class AccountModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ConcurrencyCheck]
        public string Login { get; set; }

        [Required]
        [ConcurrencyCheck]
        [MaxLength(256)]
        public byte[] Password { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [ConcurrencyCheck]
        [Index]
        [MaxLength(256)]
        public byte[] Key { get; set; }

        public virtual ICollection<BlockModel> Blocks { get; set; }

        public AccountModel()
        {
            Blocks = new List<BlockModel>();
        }


    }
}
