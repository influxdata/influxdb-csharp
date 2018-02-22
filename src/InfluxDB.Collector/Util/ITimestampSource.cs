using System;

namespace InfluxDB.Collector.Util
{
    /// <summary>
    /// Supplier of timestamps for metrics 
    /// </summary>
    interface ITimestampSource
    {
        DateTime GetUtcNow();
    }
}
