namespace Ombi.Core.Models.Requests
{
    public class RootFolderModel
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public long FreeSpace { get; set; }
    }
}