using System.IO;
using IFileSystem = EliteDangerous2SPADneXt.FileSystemAbstractions.IFileSystem;

namespace EliteDangerous2SPADneXt.Tests
{
    public class FakeFileSystem : IFileSystem
    {
        private readonly System.IO.Abstractions.IFileSystem _fileSystem;

        public FakeFileSystem(System.IO.Abstractions.IFileSystem fileSystem)
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