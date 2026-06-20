using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.FileSystemAbstractions;
using EliteDangerous2SPADneXt.GameState;
using SPAD.neXt.Interfaces.Logging;

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
        private readonly ILogger _logger;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public StatusFileChangeHandler(IFileSystem fileSystem,
            ChannelWriter<Status> channelWriter,
            ILogger logger)
        {
            _fileSystem = fileSystem;
            _channelWriter = channelWriter;
            _logger = logger;
        }

        public async Task ProcessFileUpdate(string filePath, CancellationToken cancellationToken)
        {
            if (!_fileSystem.FileExists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            using (Stream s = _fileSystem.CreateFileStream(filePath, FileMode.Open, FileAccess.Read,
                       FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(s))
                {
                    var contents = await reader.ReadToEndAsync();
                    try
                    {
                        var gameStateInfo = ParseGameStateInfo(contents);
                        if (gameStateInfo != null)
                        {
                            await ProcessStatusAsync(gameStateInfo, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"Unable to parse ED state file. Caught exception {ex}");
                        throw;
                    }
                }
            }
        }

        private async Task ProcessStatusAsync(Status gameStateInfo, CancellationToken cancellationToken = default)
        {
            await _channelWriter.WaitToWriteAsync(cancellationToken);
            await _channelWriter.WriteAsync(gameStateInfo, cancellationToken);
        }

        public void ProcessStatus(Status gameStateInfo)
        {
            ProcessStatusAsync(gameStateInfo).GetAwaiter().GetResult();
        }

        private Status ParseGameStateInfo(string contents)
        {
            if (string.IsNullOrEmpty(contents))
            {
                return null;
            }

            var state = JsonSerializer.Deserialize<Status>(contents, _jsonSerializerOptions);

            if (state == null)
            {
                throw new ParsingException("Unable to parse the ED game state file.");
            }

            return state;
        }
    }
}