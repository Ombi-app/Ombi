namespace Ombi.Core.Helpers;

public class FileSystem : IFileSystem
{
    public bool FileExists(string path)
    {
        return System.IO.File.Exists(path);
    }
    // Implement other file system operations as needed
}