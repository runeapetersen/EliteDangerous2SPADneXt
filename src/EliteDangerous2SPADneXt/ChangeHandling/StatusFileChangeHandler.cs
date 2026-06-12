using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.GameState;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EliteDangerous2SPADneXt.ChangeHandling
{
    /// <summary>
    /// Handles the processing of updates to a specified status file and
    /// facilitates parsing and transmission of the file's contents.
    /// </summary>
    public class StatusFileChangeHandler
    {
        private readonly IFileSystem _fileSystem;
        private readonly ChannelWriter<Status> _channelWriter;


        public StatusFileChangeHandler(IFileSystem fileSystem,
            ChannelWriter<Status> channelWriter)
        {
            _fileSystem = fileSystem;
            _channelWriter = channelWriter;
        }

        public async Task ProcessFileUpdate(IFileInfo file)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException($"File not found: {file.FullName}");
            }

            using (Stream s = _fileSystem.FileStream.New(file.FullName, FileMode.Open, FileAccess.Read,
                       FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(s))
                {
                    var contents = reader.ReadToEnd();
                    try
                    {
                        var gameStateInfo = ParseGameStateInfo(contents);
                        await _channelWriter.WaitToWriteAsync(CancellationToken.None);
                        await _channelWriter.WriteAsync(gameStateInfo);
                    }
                    catch (JsonReaderException)
                    {
                        throw new ParsingException("Unable to parse the ED game state file.");
                    }
                }
            }
        }

        private Status ParseGameStateInfo(string contents)
        {
            if (string.IsNullOrEmpty(contents))
            {
                return null;
            }

            var state = JsonConvert.DeserializeObject<Status>(contents, new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() }
            });

            return state;
        }
    }
}