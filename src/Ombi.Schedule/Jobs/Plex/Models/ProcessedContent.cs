using System.Collections.Generic;
using System.Linq;

namespace Ombi.Schedule.Jobs.Plex.Models
{
    public class ProcessedContent
    {
        public IEnumerable<int> Content { get; set; }
        public IEnumerable<int> Episodes { get; set; }

        public bool HasProcessedContent => Content?.Any() ?? false;
        public bool HasProcessedEpisodes => Episodes?.Any() ?? false;
    }
}