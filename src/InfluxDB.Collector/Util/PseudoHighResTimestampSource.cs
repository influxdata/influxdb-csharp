using System;

namespace InfluxDB.Collector.Util
{



    /// <summary>
    /// Implements <see cref="ITimestampSource"/>
    /// in a way that combines the low-ish resolution DateTime.UtcNow
    /// with a sequence number added to the ticks to provide 
    /// pseudo-tick precision timestamps
    /// </summary>
    /// <remarks>
    /// See https://github.com/influxdata/influxdb-csharp/issues/46 for why this is necessary.
    /// Long story short: 
    /// a) InfluxDB has a "LastWriteWins" policy for points that have the same timestamp and tags. 
    /// b) The normal <see cref="System.DateTime.UtcNow"/> only supplies timestamps that change not as often as you think.
    /// c) In a web server, it's entirely possible for more than one thread to get the same UtcNow value
    /// 
    /// As a remediation for this, we infuse DateTime.UtcNow with a sequence number until it ticks over.
    /// 
    /// </remarks>
    public class PseudoHighResTimestampSource : ITimestampSource
    {

        private long _lastUtcNowTicks = 0;
        private long _sequence = 0;
        private readonly object lockObj = new object();

       
        public DateTime GetUtcNow()
        {
            DateTime utcNow = DateTime.UtcNow;

            lock (lockObj)
            {
                if (utcNow.Ticks == _lastUtcNowTicks)
                {
                    // UtcNow hasn't rolled over yet, so 
                    // add a sequence number to it
                    _sequence++;
                    long pseudoTicks = utcNow.Ticks + _sequence;
                    return new DateTime(pseudoTicks, DateTimeKind.Utc);
                }
                else
                {
                    // Reset as UtcNow has rolled over
                    _sequence = 0;
                    _lastUtcNowTicks = utcNow.Ticks;
                    return utcNow;
                }
            }

        }


    }


}
