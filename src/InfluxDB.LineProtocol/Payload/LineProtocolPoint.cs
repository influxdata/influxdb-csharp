using System;
using System.Collections.Generic;
using System.Linq;

namespace InfluxDB.LineProtocol.Payload
{
    public class LineProtocolPoint
    {
        public string Measurement { get; }
        public IReadOnlyDictionary<string, object> Fields { get; }
        public IReadOnlyDictionary<string, string> Tags { get; }
        public DateTime? UtcTimestamp { get; }

        public LineProtocolPoint(
            string measurement,
            IReadOnlyDictionary<string, object> fields,
            IReadOnlyDictionary<string, string> tags = null,
            DateTime? utcTimestamp = null)
        {
            if (string.IsNullOrEmpty(measurement)) throw new ArgumentException("A measurement name must be specified");
            if (fields == null || fields.Count == 0) throw new ArgumentException("At least one field must be specified");

            foreach (var f in fields)
                if (string.IsNullOrEmpty(f.Key)) throw new ArgumentException("Fields must have non-empty names");

            if (tags != null)
                foreach (var t in tags)
                    if (string.IsNullOrEmpty(t.Key)) throw new ArgumentException("Tags must have non-empty names");

            if (utcTimestamp != null && utcTimestamp.Value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Timestamps must be specified as UTC");

            Measurement = measurement;
            Fields = fields;
            Tags = tags;
            UtcTimestamp = utcTimestamp;
        }

        public void Format(LineProtocolWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.Measurement(Measurement);

            if (Tags != null)
            {
                foreach (var t in Tags.OrderBy(t => t.Key))
                {
                    if (string.IsNullOrEmpty(t.Value))
                        continue;

                    writer.Tag(t.Key, t.Value);
                }
            }

            foreach (var f in Fields)
            {
                switch (f.Value)
                {
                    case sbyte value:
                        writer.Field(f.Key, value);
                        break;
                    case byte value:
                        writer.Field(f.Key, value);
                        break;
                    case short value:
                        writer.Field(f.Key, value);
                        break;
                    case ushort value:
                        writer.Field(f.Key, value);
                        break;
                    case int value:
                        writer.Field(f.Key, value);
                        break;
                    case uint value:
                        writer.Field(f.Key, value);
                        break;
                    case long value:
                        writer.Field(f.Key, value);
                        break;
                    case ulong value:
                        if (value > long.MaxValue)
                        {
                            throw new InvalidOperationException("Influx cannot store a value larger than 9223372036854775807");
                        }
                        writer.Field(f.Key, (long)value);
                        break;
                    case float value:
                        writer.Field(f.Key, value);
                        break;
                    case double value:
                        writer.Field(f.Key, value);
                        break;
                    case decimal value:
                        writer.Field(f.Key, value);
                        break;
                    case bool value:
                        writer.Field(f.Key, value);
                        break;
                    case TimeSpan value:
                        writer.Field(f.Key, value.TotalMilliseconds);
                        break;
                    default:
                        writer.Field(f.Key, f.Value.ToString());
                        break;
                }
            }

            if (UtcTimestamp != null)
            {
                writer.Timestamp(UtcTimestamp.Value);
            }
        }
    }
}

