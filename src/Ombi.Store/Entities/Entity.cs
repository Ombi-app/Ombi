using System.ComponentModel.DataAnnotations;

namespace Ombi.Store.Entities
{
    public abstract class Entity: IEntity
    {
        [Key]
        public int Id { get; set; }
    }
}