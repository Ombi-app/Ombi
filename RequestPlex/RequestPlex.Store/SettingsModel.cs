using System;

using Dapper.Contrib.Extensions;

namespace RequestPlex.Store
{
    [Table("Settings")]
    public class SettingsModel : Entity
    {
        public int Port { get; set; }
    }
}
