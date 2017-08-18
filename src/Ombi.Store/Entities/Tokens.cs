using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities
{
    public class Tokens : Entity
    {
        public string Token { get; set; }
        public string UserId { get; set; }


        [ForeignKey(nameof(UserId))]
        public OmbiUser User { get; set; }
    }
}