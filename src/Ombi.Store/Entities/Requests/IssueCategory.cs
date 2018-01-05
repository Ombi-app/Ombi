using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    [Table("IssueCategory")]
    public class IssueCategory : Entity
    {
        public string Value { get; set; }
    }
}