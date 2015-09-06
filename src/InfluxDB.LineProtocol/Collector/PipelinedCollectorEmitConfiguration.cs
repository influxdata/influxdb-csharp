using InfluxDB.LineProtocol.Client;
using System;

namespace InfluxDB.LineProtocol.Collector
{
    class PipelinedCollectorEmitConfiguration : CollectorEmitConfiguration
    {
        readonly CollectorConfiguration _configuration;
        LineProtocolClient _client;

        public PipelinedCollectorEmitConfiguration(CollectorConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _configuration = configuration;
        }

        public override CollectorConfiguration InfluxDB(Uri serverBaseAddress, string database, string username = null, string password = null)
        {
            _client = new LineProtocolClient(serverBaseAddress, database, username, password);
            return _configuration;
        }

        public IPointEmitter CreateEmitter(IPointEmitter parent, out Action dispose)
        {
            if (parent != null)
                throw new ArgumentException("Parent may not be specified here");

            if (_client == null)
            {
                dispose = null;
                return parent;
            }

            var emitter = new InfluxDBEmitter(_client);
            dispose = emitter.Dispose;
            return emitter;
        }
    }
}
