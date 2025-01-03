namespace Ombi.Core.Helpers;

public interface IFileSystem
{
    bool FileExists(string path);
    // Add other file system operations as needed
}