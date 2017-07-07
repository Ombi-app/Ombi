using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    [Table("EmailTokens")]
    public class EmailTokens : Entity
    {
        public Guid Token { get; set; }
        public int UserId { get; set; }
        public DateTime ValidUntil { get; set; }
        public bool Used { get; set; }
        public DateTime DateUsed { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
