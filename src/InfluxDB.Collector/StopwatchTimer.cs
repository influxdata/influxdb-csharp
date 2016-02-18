using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace InfluxDB.Collector
{
    class StopwatchTimer : IDisposable
    {
        readonly Stopwatch _stopwatch = new Stopwatch();
        readonly MetricsCollector _collector;
        readonly string _measurement;
        readonly IReadOnlyDictionary<string, string> _tags;

        public StopwatchTimer(MetricsCollector collector, string measurement, IReadOnlyDictionary<string, string> tags = null)
        {
            _collector = collector;
            _measurement = measurement;
            _tags = tags;
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _collector.Measure(_measurement, _stopwatch.Elapsed, _tags);
        }
    }
}
