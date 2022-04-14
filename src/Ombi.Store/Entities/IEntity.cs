using System.ComponentModel.DataAnnotations;

namespace Ombi.Store.Entities
{
    public interface IEntity
    {
        [Key]
        public int Id { get; set; }
    }
}