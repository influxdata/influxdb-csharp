using System;

namespace InfluxDB.LineProtocol.Collector
{
    class PipelinedCollectorBatchConfiguration : CollectorBatchConfiguration
    {
        readonly CollectorConfiguration _configuration;
        TimeSpan? _interval;

        public PipelinedCollectorBatchConfiguration(CollectorConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _configuration = configuration;
        }

        public override CollectorConfiguration AtInterval(TimeSpan interval)
        {
            _interval = interval;
            return _configuration;
        }

        public IPointEmitter CreateEmitter(IPointEmitter parent, out Action dispose)
        {
            if (_interval == null)
            {
                dispose = null;
                return parent;
            }

            var batcher = new IntervalBatcher(_interval.Value, parent);
            dispose = batcher.Dispose;
            return batcher;
        }
    }
}
