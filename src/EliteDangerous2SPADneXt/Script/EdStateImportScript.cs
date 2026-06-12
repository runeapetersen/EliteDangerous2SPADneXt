using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
    /// and integrates with SPAD.neXt's scripting framework by inheriting from
    /// <see cref="ScriptStub"/>.
    /// </remarks>
    // ReSharper disable once UnusedType.Global
    public class EdStateImportScript : ScriptStub, IScriptAction2, IHasID
    {
        private readonly string _locationOverrideFile = "location_override.json";

        private readonly string _defaultStatusFileLocation = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Saved Games", "Frontier Developments", "Elite Dangerous", "Status.json");

        public int NumberOfParameters => 0;

        private static readonly Guid Guid = Guid.Parse("019ead6a-f32d-75bf-88f4-47ac23832002");
        public Guid ID => Guid;
        private IDisposable _watcher;
        private IFileSystem _fileSystem = new FileSystem();
        private Channel<Status> _channel;
        private Task<int> _consumer;

        public void Execute(IApplication app, ISPADEventArgs eventArgs)
        {
            throw new NotImplementedException($"Only using {nameof(IScriptAction2)} execution logic");
        }

        protected override void InitializeScript()
        {
            var statusFileLocation = _defaultStatusFileLocation;
            var assemblyLocation = Path.GetDirectoryName(GetType().Assembly.Location);
            ScriptLogger.Debug($"Assembly location: {assemblyLocation}");
            var overrideFilePath = Path.Combine(assemblyLocation, _locationOverrideFile);

            _channel = Channel.CreateUnbounded<Status>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false,
                });

            if (!File.Exists(overrideFilePath))
            {
                ScriptLogger.Warn(
                    $"Unable to read location override file: {overrideFilePath}. Assuming the Elite Dangerous status file location is in the default file path {_defaultStatusFileLocation}");
            }
            else
            {
                try
                {
                    var overrideFileContents = File.ReadAllText(overrideFilePath);
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
            IFileSystemWatcher watcher = _fileSystem.FileSystemWatcher.New(fi.Directory.FullName, fi.Name);
            watcher.Changed += async (sender, args) => { await ChangeHandling(sender, args, changeHandler); };
            watcher.EnableRaisingEvents = true;
            _watcher = watcher;
            ScriptLogger.Info($"Watcher initialized for file {fi.FullName}");

            //consumerTask
            _consumer = Task.Run(() => ConsumeQueue(_channel.Reader, CancelToken));
        }

        private async Task<int> ConsumeQueue(ChannelReader<Status> channelReader, CancellationToken cancelToken)
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

            while (!cancelToken.IsCancellationRequested)
            {
                ScriptLogger.Info("Consumer task waiting for new objects on blocking collection");
                // awaits new objects on blocking collection
                var goAhead = await channelReader.WaitToReadAsync(cancelToken);
                ScriptLogger.Info("Wait to read completed...");
                if (!goAhead) continue;
                var status = await channelReader.ReadAsync(cancelToken);
                ScriptLogger.Info("Read completed...");

                ScriptLogger.Info("Consumer task processing new object on blocking collection");
                try
                {
                    var changedStateFields = stateContainer.Apply(status);
                    var changedFlags = edFlagsChangeHandler.HandleUpdate(status.Flags);
                    var changedFlags2 = edFlags2ChangeHandler.HandleUpdate(status.Flags2);

                    foreach (var updatedValue in changedStateFields.Concat(changedFlags2).Concat(changedFlags))
                    {
                        dataDefinitions[updatedValue.Name].Monitorable.SetValue(updatedValue.Value, 0, ID);
                    }
                }
                catch (Exception ex)
                {
                    ScriptLogger.Error($"Caught exception trying to handle message: {ex.Message}");
                }

                ScriptLogger.Info("Consumer task processed new object on blocking collection");
            }

            ScriptLogger.Info("Consumer task exiting");
            return 0;
        }

        private async Task ChangeHandling(object sender, FileSystemEventArgs eventArgs,
            StatusFileChangeHandler changeHandler)
        {
            ScriptLogger.Info("Change handler handling file change");
            if (CancelToken.IsCancellationRequested)
            {
                return;
            }

            if (eventArgs.ChangeType == WatcherChangeTypes.Changed ||
                eventArgs.ChangeType == WatcherChangeTypes.Created)
            {
                ScriptLogger.Info($"Status file changed. Reloading.");
                var statusFileInfo = _fileSystem.FileInfo.New(eventArgs.FullPath);
                await changeHandler.ProcessFileUpdate(statusFileInfo);
            }
        }

        private async Task
            action(CancellationToken token) // no longer relevant. Equivalent logic is handled in the file watcher callback
        {
            var random = new Random();
            var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Saved Games", "Frontier Developments", "Elite Dangerous", "Status.json");

            var _someAmazingField = GetOrCreateScriptDataValue("AMAZING_VAR", 0);
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(5000, token); // wait for script to terminate
                _someAmazingField.Monitorable.SetValue(random.Next(1, 1000), 0, ID);
                FileInfo info = new System.IO.FileInfo(fileName);
                ScriptLogger.Info($"File {fileName} exists: {info.Exists} and is {info.Length} bytes");
            }
        }

        protected override void DeinitializeScript()
        {
            if (_watcher == null) return;
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
            // do nothing? This script will auto-run and perform very little work if ED is not running and generating new state files
        }
    }
}