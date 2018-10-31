using System.ComponentModel.DataAnnotations.Schema;

namespace Ombi.Store.Entities.Requests
{
    [Table("IssueCategory")]
    public class IssueCategory : Entity
    {
        public string Name { get; set; }
        public string SubjectPlaceholder { get; set; }
        public string DescriptionPlaceholder { get; set; }
    }
}