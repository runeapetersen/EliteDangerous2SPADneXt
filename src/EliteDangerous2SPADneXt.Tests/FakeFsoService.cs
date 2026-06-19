using System.IO;
using System.IO.Abstractions;
using EliteDangerous2SPADneXt.FileSystemAbstractions;

namespace EliteDangerous2SPADneXt.Tests
{
    public class FakeFsoService : IFsoService
    {
        private readonly IFileSystem _fileSystem;

        public FakeFsoService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool FileExists(string filePath)
        {
            return _fileSystem.File.Exists(filePath);
        }

        public Stream CreateFileStream(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return _fileSystem.FileStream.New(filePath, fileMode, fileAccess, fileShare);
        }

        public string CombinePath(params string[] parts) => _fileSystem.Path.Combine(parts);

        public string GetDirectoryName(string path) => _fileSystem.Path.GetDirectoryName(path);
        public string ReadAllText(string path) => File.ReadAllText(path);
    }
}