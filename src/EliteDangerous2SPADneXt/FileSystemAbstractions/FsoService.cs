using System.IO;

namespace EliteDangerous2SPADneXt.FileSystemAbstractions
{
    public interface IFsoService
    {
        bool FileExists(string filePath);
        Stream CreateFileStream(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);

        string CombinePath(params string[] parts);
        string GetDirectoryName(string path);
        string ReadAllText(string path);
    }

    public class FsoService : IFsoService
    {
        public bool FileExists(string filePath) => File.Exists(filePath);
        public Stream CreateFileStream(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare) =>
            new FileStream(filePath, fileMode, fileAccess, fileShare);
        
        public string CombinePath(params string[] parts) => Path.Combine(parts);
        public string GetDirectoryName(string path) => Path.GetDirectoryName(path);
        public string ReadAllText(string path) => File.ReadAllText(path);
    }
}