using System.Collections.Generic;

namespace Ombi.Models.Identity
{
    public class SaveWizardResult
    {
        public bool Result { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}