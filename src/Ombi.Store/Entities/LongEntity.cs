using System.ComponentModel.DataAnnotations;

namespace Ombi.Store.Entities
{
    /// <summary>
    /// Base type for high-churn cache tables whose auto-increment primary keys
    /// can exceed <see cref="int.MaxValue"/> (see #5224).
    /// </summary>
    public abstract class LongEntity : IEntity
    {
        [Key]
        public long Id { get; set; }
    }
}
