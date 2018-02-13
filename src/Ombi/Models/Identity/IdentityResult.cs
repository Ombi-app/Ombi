using System.Collections.Generic;

namespace Ombi.Models.Identity
{
    public class IdentityResult
    {
        public List<string> Errors { get; set; }
        public bool Successful { get; set; }
    }
}