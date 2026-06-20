using System.IO;

namespace EliteDangerous2SPADneXt.FileSystemAbstractions
{
    public interface IFileSystem
    {
        bool FileExists(string filePath);
        Stream CreateFileStream(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);

        string CombinePath(params string[] parts);
        string GetDirectoryName(string path);
        string ReadAllText(string path);
    }
}