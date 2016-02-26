using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper.Contrib.Extensions;

namespace RequestPlex.Store
{
    public class Entity
    {
        [Key]
        public int Id { get; set; }
    }
}
