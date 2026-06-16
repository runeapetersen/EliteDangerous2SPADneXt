using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.GameState;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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


        public StatusFileChangeHandler(IFileSystem fileSystem,
            ChannelWriter<Status> channelWriter,
            ILogger logger)
        {
            _fileSystem = fileSystem;
            _channelWriter = channelWriter;
            _logger = logger;
        }

        public async Task ProcessFileUpdate(IFileInfo file, CancellationToken cancellationToken)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException($"File not found: {file.FullName}");
            }

            using (Stream s = _fileSystem.FileStream.New(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(s))
                {
                    var contents = await reader.ReadToEndAsync();
                    try
                    {
                        var gameStateInfo = ParseGameStateInfo(contents);
                        if (gameStateInfo != null)
                        {
                            await ProcessStatus(gameStateInfo, cancellationToken);
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

        public async Task ProcessStatus(Status gameStateInfo, CancellationToken cancellationToken)
        {
            await _channelWriter.WaitToWriteAsync(cancellationToken);
            await _channelWriter.WriteAsync(gameStateInfo, cancellationToken);
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

            if (state == null)
            {
                throw new ParsingException("Unable to parse the ED game state file.");
            }
            
            return state;
        }
    }
}