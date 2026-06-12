using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Channels;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.ChangeHandling;
using EliteDangerous2SPADneXt.GameState;
using Xunit;

namespace EliteDangerous2SPADneXt.Tests
{
    public class StatusFileChangeHandlerTests
    {
        [Fact]
        public async Task CanProcessFileUpdates_NonExistingFile_Throws()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
            });
            var channel = Channel.CreateUnbounded<Status>();
            var sut = new StatusFileChangeHandler(fileSystem, channel.Writer);
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                sut.ProcessFileUpdate(fileSystem.FileInfo.New(@"d:\not_a_file.json")));
        }

        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] { @"TestData\Status_Empty.json" };
            yield return new object[] { @"TestData\Status_Docked.json" };
            yield return new object[] { @"TestData\Status_InSpace.json" };
            yield return new object[] { @"TestData\Status_OnFoot.json" };
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task CanProcessFileUpdates_ExistingFile_DoesNotThrow(string testData)
        {
            var testFileContent = File.ReadAllText(testData);
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData(testFileContent) }
            });
            var channel = Channel.CreateUnbounded<Status>();
            var sut = new StatusFileChangeHandler(fileSystem, channel.Writer);
            await sut.ProcessFileUpdate(fileSystem.FileInfo.New(@"c:\myfile.txt"));
            Assert.True(channel.Reader.Count==1);
        }

        [Fact]
        public async Task CanProcessFileUpdates_ExistingFile_CannotParse_Throws()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
            });
            var channel = Channel.CreateUnbounded<Status>();
            var sut = new StatusFileChangeHandler(fileSystem, channel.Writer);
            await Assert.ThrowsAsync<ParsingException>(() => sut.ProcessFileUpdate(fileSystem.FileInfo.New(@"c:\myfile.txt")));
        }

        [Fact]
        public async Task CanProcessFileUpdates()
        {
        }
    }
}