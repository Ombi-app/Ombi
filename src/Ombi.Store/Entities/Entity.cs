using System.ComponentModel.DataAnnotations;

namespace Ombi.Store.Entities
{
    public abstract class Entity
    {
        [Key]
        public int Id { get; set; }
    }
}