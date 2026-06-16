using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.ChangeHandling;
using EliteDangerous2SPADneXt.GameState;
using SPAD.neXt.Interfaces.Configuration;
using SPAD.neXt.Interfaces.Logging;

namespace EliteDangerous2SPADneXt.Script
{
    public class StatusConsumer
    {
        private readonly Guid _scriptId;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, IDataDefinition> _dataDefinitions = new ConcurrentDictionary<string, IDataDefinition>();
        private readonly Func<string, object, IDataDefinition> _getOrCreateScriptDataValue;

        public StatusConsumer(
            Guid scriptId,
            Func<string, object, IDataDefinition> getOrCreateScriptDataValue,
            ILogger logger)
        {
            _scriptId = scriptId;
            _getOrCreateScriptDataValue = getOrCreateScriptDataValue;
            _logger = logger;
        }

        public Task<int> Start(
            ChannelReader<Status> channelReader,
            CancellationToken cancellationToken)
        {
            if (channelReader == null)
            {
                throw new ArgumentNullException(nameof(channelReader));
            }

            return Task.Run(() => ConsumeQueue(channelReader, cancellationToken, _logger), cancellationToken);
        }

        public async Task<int> ConsumeQueue(
            ChannelReader<Status> channelReader,
            CancellationToken cancellationToken,
            ILogger logger)
        {
            var stateContainer = new StateContainer(logger);
            logger.Info("Consumer task waiting for new objects in channel");

            while (await channelReader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                logger.Info("Wait to read completed...");
                    
                while (channelReader.TryRead(out var status) && !cancellationToken.IsCancellationRequested)
                {
                    ProcessStatusUpdate(status, stateContainer);
                }
            }

            logger.Info("Consumer task exiting");
            return 0;
        }

        private void ProcessStatusUpdate(
            Status status,
            StateContainer stateContainer)
        {
            _logger.Info("Consumer task processing new object");

            var changedStateFields = stateContainer.Apply(status);

            foreach (var updatedValue in changedStateFields)
            {
                Func<string, IDataDefinition> factory;
                switch (updatedValue.DataType)
                {
                    case SpadDataType.BOOL:
                        factory = name =>
                        {
                            var dataDef = _getOrCreateScriptDataValue(name, false);
                            dataDef.UnitsName = nameof(SpadDataType.BOOL);
                            return dataDef;
                        };
                        break;
                    case SpadDataType.STRING:
                        factory = name =>
                        {
                            var dataDef = _getOrCreateScriptDataValue(name, string.Empty);
                            dataDef.UnitsName = nameof(SpadDataType.STRING);
                            return dataDef;
                        };
                        break;
                    case SpadDataType.NUMBER:
                        factory = name =>
                        {
                            var dataDef = _getOrCreateScriptDataValue(name, 0);
                            dataDef.UnitsName = nameof(SpadDataType.NUMBER);
                            return dataDef;
                        };
                        break;
                    default:
                        throw new NotSupportedException("Unsupported Datatype");
                }

                var dataDefinition = _dataDefinitions.GetOrAdd(updatedValue.Name, factory);
                dataDefinition.Monitorable.SetValue(updatedValue.Value, 0, _scriptId);
            }

            _logger.Info("Consumer task finished processing new object");
        }
    }
}