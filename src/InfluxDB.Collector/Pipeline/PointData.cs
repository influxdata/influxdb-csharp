using System;
using System.Collections.Generic;
using System.Linq;

namespace InfluxDB.Collector.Pipeline
{
    public class PointData
    {
        public string Measurement { get; }
        public Dictionary<string, object> Fields { get; }
        public Dictionary<string, string> Tags { get; set; }
        public DateTime? UtcTimestamp { get; }

        public PointData(
            string measurement,
            IReadOnlyDictionary<string, object> fields,
            IReadOnlyDictionary<string, string> tags = null,
            DateTime? utcTimestamp = null)
        {
            if (measurement == null) throw new ArgumentNullException(nameof(measurement));
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            Measurement = measurement;
            Fields = fields.ToDictionary(kv => kv.Key, kv => kv.Value);
            if (tags != null)
                Tags = tags.ToDictionary(kv => kv.Key, kv => kv.Value);
            UtcTimestamp = utcTimestamp;
        }
    }
}
