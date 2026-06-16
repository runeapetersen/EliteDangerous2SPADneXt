using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EliteDangerous2SPADneXt.GameState;
using EliteDangerous2SPADneXt.Script;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SPAD.neXt.Interfaces.Configuration;
using SPAD.neXt.Interfaces.Events;
using SPAD.neXt.Interfaces.Logging;
using Xunit;

namespace EliteDangerous2SPADneXt.Tests
{
    public class StatusConsumerTests
    {
        private readonly Mock<IDataDefinition> _dataDefinitionMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IMonitorableValue> _monitorableValue;

        public StatusConsumerTests()
        {
            _monitorableValue = new Mock<IMonitorableValue>();
            _dataDefinitionMock = new Mock<IDataDefinition>();
            _dataDefinitionMock.SetupGet(m => m.Monitorable).Returns(_monitorableValue.Object); 
            _loggerMock = new Mock<ILogger>();
        }
        
        [Fact]
        public async Task BasicConsumerTest()
        {
            var channel = Channel.CreateUnbounded<Status>();
            var createdDefinitions = new Dictionary<string, IDataDefinition>();

            var consumer = new StatusConsumer(
                Guid.NewGuid(),
                (name, defaultValue) =>
                {
                    var definition = _dataDefinitionMock.Object;
                    createdDefinitions.Add(name, definition);
                    return definition;
                },
                _loggerMock.Object);

            await channel.Writer.WriteAsync(new Status());
            var contents =
                @"{ ""timestamp"":""2026-06-14T22:06:29Z"", ""event"":""Status"", ""Flags"":16842765, ""Flags2"":0, ""Pips"":[4,4,4], ""FireGroup"":0, ""GuiFocus"":0, ""Fuel"":{ ""FuelMain"":2.000000, ""FuelReservoir"":0.300000 }, ""Cargo"":0.000000, ""LegalState"":""Clean"", ""Balance"":0 }";
            var state = JsonConvert.DeserializeObject<Status>(contents, new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() }
            });
            
            await channel.Writer.WriteAsync(state);
            
            channel.Writer.Complete();

            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await consumer.ConsumeQueue(channel.Reader, cts.Token, _loggerMock.Object);
            
            Assert.True(channel.Reader.Count == 0);
            Assert.True(createdDefinitions.Count>1);
            Assert.True(_dataDefinitionMock.Invocations.Count > 10);
        }
    }
}