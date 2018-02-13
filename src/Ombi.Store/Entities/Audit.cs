using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ombi.Store.Entities
{
    [Table("Audit")]
    public class Audit : Entity
    {
        public DateTime DateTime { get; set; }
        public string Description { get; set; }
        public AuditType AuditType { get; set; }
        public AuditArea AuditArea { get; set; }
        public string User { get; set; }
    }

    public enum AuditType
    {
        None,
        Created,
        Updated,
        Deleted,
        Approved,
        Denied,
        Success,
        Fail,
        Added
    }

    public enum AuditArea
    {
        Authentication,
        User,
        TvRequest,
        MovieRequest,
    }
}
