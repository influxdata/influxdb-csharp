using System;

namespace InfluxDB.LineProtocol.Collector
{
    public abstract class CollectorBatchConfiguration
    {
        public abstract CollectorConfiguration AtInterval(TimeSpan interval);
    }
}
