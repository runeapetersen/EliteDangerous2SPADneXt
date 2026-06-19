using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.ChangeHandling;
using EliteDangerous2SPADneXt.GameState;
using Moq;
using SPAD.neXt.Interfaces.Logging;
using Xunit;

namespace EliteDangerous2SPADneXt.Tests
{
    public class StatusFileChangeHandlerTests
    {
        private Mock<ILogger> _loggerMock = new Mock<ILogger>();

        [Fact]
        public async Task CanProcessFileUpdates_NonExistingFile_Throws()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
            });
            var fso = new FakeFsoService(fileSystem);
            var channel = Channel.CreateUnbounded<Status>();
            var sut = new StatusFileChangeHandler(fso, channel.Writer, _loggerMock.Object);
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                sut.ProcessFileUpdate(@"d:\not_a_file.json", CancellationToken.None));
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
            var fso = new FakeFsoService(fileSystem);
            var channel = Channel.CreateUnbounded<Status>();
            var sut = new StatusFileChangeHandler(fso, channel.Writer, _loggerMock.Object);
            await sut.ProcessFileUpdate(@"c:\myfile.txt", CancellationToken.None);
            Assert.True(channel.Reader.Count == 1);
        }

        [Fact]
        public async Task CanProcessFileUpdates_ExistingFile_CannotParse_NothingProduced()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
            });
            var fso = new FakeFsoService(fileSystem);
            var channel = Channel.CreateUnbounded<Status>();
            var sut = new StatusFileChangeHandler(fso, channel.Writer, _loggerMock.Object);
            Assert.True(channel.Reader.Count == 0);
        }
    }
}