using System;

namespace InfluxDB.Collector.Configuration
{
    public abstract class CollectorBatchConfiguration
    {
        public abstract CollectorConfiguration AtInterval(TimeSpan interval);
    }
}
