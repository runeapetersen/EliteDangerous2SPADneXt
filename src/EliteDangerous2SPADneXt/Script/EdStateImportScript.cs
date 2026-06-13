using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.ChangeHandling;
using EliteDangerous2SPADneXt.Configuration;
using EliteDangerous2SPADneXt.GameState;
using Newtonsoft.Json;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Configuration;
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
        private static readonly Guid ScriptGuid = Guid.Parse("019ead6a-f32d-75bf-88f4-47ac23832002");
        private static readonly string LocationOverrideFile = "location_override.json";
        private readonly string _defaultStatusFileLocation;
        private IDisposable _watcher;
        private readonly IFileSystem _fileSystem;
        private Channel<Status> _channel;
        private Task<int> _consumer;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public Guid ID => ScriptGuid;
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
            var statusFileLocation = _defaultStatusFileLocation;
            var assemblyLocation = _fileSystem.Path.GetDirectoryName(GetType().Assembly.Location);
            if (string.IsNullOrEmpty(assemblyLocation))
                ScriptLogger.Warn("Unable to discern assembly file location.");
            var overrideFilePath = _fileSystem.Path.Combine(assemblyLocation ?? string.Empty, LocationOverrideFile);

            _channel = Channel.CreateBounded<Status>(
                new BoundedChannelOptions(20)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.DropOldest
                });

            if (!_fileSystem.File.Exists(overrideFilePath))
            {
                ScriptLogger.Warn(
                    $"Unable to read location override file: {overrideFilePath}. Assuming the Elite Dangerous status file location is in the default file path {_defaultStatusFileLocation}");
            }
            else
            {
                try
                {
                    var overrideFileContents = _fileSystem.File.ReadAllText(overrideFilePath);
                    var overrideFileContentsJson =
                        JsonConvert.DeserializeObject<OverrideFileContents>(overrideFileContents);
                    if (!string.IsNullOrEmpty(overrideFileContentsJson.StatusFilePathOverride))
                    {
                        statusFileLocation = overrideFileContentsJson.StatusFilePathOverride;
                    }
                }
                catch (Exception ex)
                {
                    ScriptLogger.Warn(
                        $"Caught exception '{ex.Message}'. Unable to read location override file: {overrideFilePath}. Assuming the Elite Dangerous status file location is in the default file path {_defaultStatusFileLocation}");
                }
            }

            StatusFileChangeHandler changeHandler = new StatusFileChangeHandler(_fileSystem, _channel.Writer);

            IFileInfo fi = _fileSystem.FileInfo.New(statusFileLocation);
            if (fi.Directory == null)
                throw new ApplicationException("Unable to determine directory for status file");

            IFileSystemWatcher watcher = _fileSystem.FileSystemWatcher.New(fi.Directory.FullName, fi.Name);
            watcher.Changed += async (sender, args) => { await ChangeHandling(sender, args, changeHandler); };
            watcher.Created += async (sender, args) => { await ChangeHandling(sender, args, changeHandler); };
            watcher.EnableRaisingEvents = true;
            _watcher = watcher;
            ScriptLogger.Info($"Watcher initialized for file {fi.FullName}");

            //consumerTask
            _consumer = Task.Run(() => ConsumeQueue(_channel.Reader, _cancellationTokenSource.Token));
        }

        private async Task<int> ConsumeQueue(ChannelReader<Status> channelReader, CancellationToken cancellationToken)
        {
            ScriptLogger.Info("Consumer task starting");
            StateContainer stateContainer = new StateContainer();
            FlagChangeHandler<EdFlags> edFlagsChangeHandler = new FlagChangeHandler<EdFlags>(0);
            FlagChangeHandler<EdFlags2> edFlags2ChangeHandler = new FlagChangeHandler<EdFlags2>(0);

            var initialDataPackage = stateContainer.GetCurrentValues()
                .Concat(edFlagsChangeHandler.GetCurrentValues())
                .Concat(edFlags2ChangeHandler.GetCurrentValues());

            Dictionary<string, IDataDefinition> dataDefinitions = new Dictionary<string, IDataDefinition>();

            foreach (var updatedValue in initialDataPackage)
            {
                var newDefinition = GetOrCreateScriptDataValue(updatedValue.Name,
                    updatedValue.DataType == SpadDataType.NUMBER ? (object)0 : string.Empty);
                newDefinition.UnitsName = updatedValue.DataType.ToString();
                newDefinition.Monitorable.SetValue(updatedValue.Value, 0, ID);
                dataDefinitions.Add(updatedValue.Name, newDefinition);
            }

            ScriptLogger.Info("Initial data variables seeded by consumer task.");
            ScriptLogger.Info("Consumer task waiting for new objects in channel");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    while (await channelReader.WaitToReadAsync(cancellationToken))
                    {
                        ScriptLogger.Info("Wait to read completed...");
                        while (channelReader.TryRead(out var status))
                        {
                            ScriptLogger.Info("Consumer task processing new object");

                            var changedStateFields = stateContainer.Apply(status);
                            var changedFlags = edFlagsChangeHandler.HandleUpdate(status.Flags);
                            var changedFlags2 = edFlags2ChangeHandler.HandleUpdate(status.Flags2);

                            foreach (var updatedValue in changedStateFields.Concat(changedFlags2).Concat(changedFlags))
                            {
                                dataDefinitions[updatedValue.Name].Monitorable.SetValue(updatedValue.Value, 0, ID);
                            }
                            
                            ScriptLogger.Info("Consumer task finished processing new object");
                        }
                    }
                }
                catch(Exception ex)
                {
                    ScriptLogger.Error($@"Caught exception {ex} while trying to consume status message.");
                    if (ex is TaskCanceledException)
                    {
                        break;
                    }

                    throw;
                }
            }

            ScriptLogger.Info("Consumer task exiting");
            return 0;
        }

        private async Task ChangeHandling(object _, System.IO.FileSystemEventArgs eventArgs,
            StatusFileChangeHandler changeHandler)
        {
            ScriptLogger.Info("Change handler handling file change");

            if (eventArgs.ChangeType == System.IO.WatcherChangeTypes.Changed ||
                eventArgs.ChangeType == System.IO.WatcherChangeTypes.Created)
            {
                ScriptLogger.Info($"Status file changed. Reloading.");
                var statusFileInfo = _fileSystem.FileInfo.New(eventArgs.FullPath);
                try
                {
                    await changeHandler.ProcessFileUpdate(statusFileInfo);
                }
                catch(Exception ex)
                {
                    ScriptLogger.Error($@"Caught exception {ex} while trying to process status file.");
                    throw;
                }
            }
        }

        protected override void DeinitializeScript()
        {
            _channel.Writer.TryComplete();
            if (_watcher == null) return;
            try
            {
                _watcher.Dispose();
            }
            finally
            {
                _watcher = null;
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        protected override string ScriptDataPrefix => "ED2SPADNEXT";

        public void Execute(IApplication app, List<IEventActionParameter> actionParameters)
        {
            // do nothing? This script will auto-run and perform very little work if ED is not running and generating new state files
        }
    }
}