namespace Ombi.Core.Models.Search.V2.Music
{
    public class AlbumArt
    {
        public AlbumArt()
        {
            
        }

        public AlbumArt(string url)
        {
            Image = url;
        }
        public string Image { get; set; }
    }
}