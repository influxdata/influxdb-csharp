using System;

namespace InfluxDB.Collector.Util
{
    /// <summary>
    /// Supplier of timestamps for metrics 
    /// </summary>
    internal interface ITimestampSource
    {
        DateTime GetUtcNow();
    }
}
