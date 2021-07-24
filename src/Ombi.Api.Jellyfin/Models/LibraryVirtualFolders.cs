using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Api.Jellyfin.Models
{
    public class LibraryVirtualFolders
    {
        public string Name { get; set; }
        public Libraryoptions LibraryOptions { get; set; }
        public string ItemId { get; set; }
    }

    public class Libraryoptions
    {
        public List<Typeoption> TypeOptions { get; set; } = new List<Typeoption>();
    }

    public class Typeoption
    {
        public string Type { get; set; }
    }

}
