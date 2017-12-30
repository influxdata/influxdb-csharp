using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
