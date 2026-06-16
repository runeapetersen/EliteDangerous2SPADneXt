using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.ChangeHandling;
using EliteDangerous2SPADneXt.Configuration;
using EliteDangerous2SPADneXt.GameState;
using Newtonsoft.Json;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Events;
using SPAD.neXt.Interfaces.Scripting;
using SPAD.neXt.Interfaces.Scripting.Stubs;

namespace EliteDangerous2SPADneXt.Script
{
    /// <summary>
    /// Represents a script implementation for importing and processing
    /// Elite Dangerous status file updates in the context of SPAD.neXt integration.
    /// </summary>
    /// <remarks>
    /// This class is designed to monitor changes to the Elite Dangerous status file
    /// and handle them through a custom change handler and an asynchronous queue consumer.
    /// It implements <see cref="IScriptAction2"/> to allow script-driven execution
    /// and integrates with the SPAD.neXt scripting framework by inheriting from
    /// <see cref="ScriptStub"/>.
    /// </remarks>
    // ReSharper disable once UnusedType.Global
    public class EdStateImportScript : ScriptStub, IScriptAction2, IHasID
    {
        private static readonly string LocationOverrideFile = "location_override.json";
        private const int ChannelCapacity = 20;

        private readonly string _defaultStatusFileLocation;
        private readonly IFileSystem _fileSystem;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private IDisposable _watcher;
        private Task<int> _consumer;
        private Channel<Status> _channel;
        private StatusConsumer _statusConsumer;

        public Guid ID => Guid.Parse("019ead6a-f32d-75bf-88f4-47ac23832002");
        public int NumberOfParameters => 0;

        public EdStateImportScript()
        {
            _fileSystem = new FileSystem();
            _defaultStatusFileLocation = _fileSystem.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Saved Games", "Frontier Developments", "Elite Dangerous", "Status.json");
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(CancelToken);
        }

        public void Execute(IApplication app, ISPADEventArgs eventArgs)
        {
            throw new NotImplementedException($"Only using {nameof(IScriptAction2)} execution logic");
        }

        protected override void InitializeScript()
        {
            var statusFileLocation = ResolveStatusFileLocation();

            _channel = CreateStatusChannel();

            var changeHandler = new StatusFileChangeHandler(_fileSystem, _channel.Writer, ScriptLogger.CreateChildLogger(nameof(StatusFileChangeHandler)));
            
            // Pre-seed the system with default values...
            try
            {
                Task.Run(() => changeHandler.ProcessStatus(new Status { TimeStamp = DateTimeOffset.Now }, _cancellationTokenSource.Token))
                    .Wait(1000, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                ScriptLogger.Warn($"Failed to pre-seed channel with initial status: {ex.Message}");
            }
            
            _watcher = CreateStatusFileWatcher(statusFileLocation, changeHandler);

            _statusConsumer = new StatusConsumer(
                ID,
                GetOrCreateScriptDataValue,
                ScriptLogger.CreateChildLogger(nameof(StatusConsumer)));
            _consumer = _statusConsumer.Start(_channel.Reader, _cancellationTokenSource.Token);
        }

        private string ResolveStatusFileLocation()
        {
            var overrideFilePath = GetOverrideFilePath();

            if (!_fileSystem.File.Exists(overrideFilePath))
            {
                LogOverrideFileUnavailable(overrideFilePath);
                return _defaultStatusFileLocation;
            }

            return ReadStatusFileLocationOverride(overrideFilePath) ?? _defaultStatusFileLocation;
        }
        
        private string GetOverrideFilePath()
        {
            var assemblyLocation = _fileSystem.Path.GetDirectoryName(GetType().Assembly.Location);

            if (string.IsNullOrEmpty(assemblyLocation))
            {
                ScriptLogger.Warn("Unable to discern assembly file location.");
            }

            return _fileSystem.Path.Combine(assemblyLocation ?? string.Empty, LocationOverrideFile);
        }

        private string ReadStatusFileLocationOverride(string overrideFilePath)
        {
            try
            {
                var overrideFileContents = _fileSystem.File.ReadAllText(overrideFilePath);
                var overrideFileContentsJson =
                    JsonConvert.DeserializeObject<OverrideFileContents>(overrideFileContents);

                return string.IsNullOrEmpty(overrideFileContentsJson?.StatusFilePathOverride)
                    ? null
                    : overrideFileContentsJson.StatusFilePathOverride;
            }
            catch (Exception ex)
            {
                ScriptLogger.Warn(
                    $"Caught exception '{ex}'. Unable to read location override file: {overrideFilePath}. Assuming the Elite Dangerous status file location is in the default file path {_defaultStatusFileLocation}");

                return null;
            }
        }

        private void LogOverrideFileUnavailable(string overrideFilePath)
        {
            ScriptLogger.Warn(
                $"Unable to read location override file: {overrideFilePath}. Assuming the Elite Dangerous status file location is in the default file path {_defaultStatusFileLocation}");
        }

        private static Channel<Status> CreateStatusChannel()
        {
            return Channel.CreateBounded<Status>(
                new BoundedChannelOptions(ChannelCapacity)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.DropOldest
                });
        }

        private IDisposable CreateStatusFileWatcher(
            string statusFileLocation,
            StatusFileChangeHandler changeHandler)
        {
            var statusFileInfo = _fileSystem.FileInfo.New(statusFileLocation);

            if (statusFileInfo.Directory == null)
            {
                throw new ApplicationException("Unable to determine directory for status file");
            }

            var watcher = _fileSystem.FileSystemWatcher.New(
                statusFileInfo.Directory.FullName,
                statusFileInfo.Name);

            watcher.Changed += async (sender, args) => await ChangeHandling(sender, args, changeHandler);
            //watcher.Created += async (sender, args) => await ChangeHandling(sender, args, changeHandler);
            watcher.EnableRaisingEvents = true;

            ScriptLogger.Info($"Watcher initialized for file {statusFileInfo.FullName}");

            return watcher;
        }

        private async Task ChangeHandling(
            object _,
            System.IO.FileSystemEventArgs eventArgs,
            StatusFileChangeHandler changeHandler)
        {
            if (!IsRelevantChange(eventArgs.ChangeType))
            {
                return;
            }

            ScriptLogger.Info($"Change handler handling file change for {eventArgs.FullPath}");

            var statusFileInfo = _fileSystem.FileInfo.New(eventArgs.FullPath);
            
            try
            {
                await changeHandler.ProcessFileUpdate(statusFileInfo, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                ScriptLogger.Error($@"Caught exception {ex} while trying to process status file.");
            }
        }

        private static bool IsRelevantChange(System.IO.WatcherChangeTypes changeType)
        {
            return changeType == System.IO.WatcherChangeTypes.Changed ||
                   changeType == System.IO.WatcherChangeTypes.Created;
        }

        protected override void DeinitializeScript()
        {
            _channel?.Writer.TryComplete();

            DisposeWatcher();

            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
        }

        private void DisposeWatcher()
        {
            if (_watcher == null)
            {
                return;
            }

            try
            {
                _watcher.Dispose();
            }
            finally
            {
                _watcher = null;
            }
        }

        protected override string ScriptDataPrefix => "ED2SPADNEXT";

        public void Execute(IApplication app, List<IEventActionParameter> actionParameters)
        {
            // no-op
        }
    }
}