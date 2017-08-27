using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using InfluxDB.LineProtocol;
using InfluxDB.LineProtocol.Payload;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class WriteLineProtocol
    {
        private const int N = 500;

        private static readonly string[] Colours = { "red", "blue", "green" };

        private readonly (DateTime timestamp, string colour, double value)[] data;

        public WriteLineProtocol()
        {
            var random = new Random(755);
            var now = DateTime.UtcNow;
            data = Enumerable.Range(0, N).Select(i => (now.AddMilliseconds(random.Next(2000)), Colours[random.Next(Colours.Length)], random.NextDouble())).ToArray();
        }

        [Benchmark(Baseline = true)]
        public string LineProtocolPoint()
        {
            var payload = new LineProtocolPayload();

            foreach (var point in data)
            {
                payload.Add(new LineProtocolPoint(
                    "example",
                    new Dictionary<string, object>
                    {
                        {"value", point.value}
                    },
                    new Dictionary<string, string>
                    {
                        {"colour", point.colour}
                    },
                    point.timestamp
                ));
            }

            var writer = new StringWriter();
            payload.Format(writer);
            return writer.ToString();
        }

        [Benchmark]
        public string LineProtocolWriter()
        {
            var writer = new LineProtocolWriter();

            foreach (var point in data)
            {
                writer.Measurement("example").Tag("colour", point.colour).Field("value", point.value).Timestamp(point.timestamp);
            }

            return writer.ToString();
        }

        [Benchmark]
        public string StringInterpolation()
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var lines = new List<string>();

            foreach (var point in data)
            {
                var timestamp = point.timestamp - unixEpoch;
                lines.Add($"example,colour={point.colour} value={point.value} {timestamp.Ticks * 100L}");
            }

            return string.Join("\n", lines);
        }

        [Benchmark]
        public string StringBuilder()
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var lines = new StringBuilder();

            foreach (var point in data)
            {
                var timestamp = point.timestamp - unixEpoch;
                lines.Append("example,colour=").Append(point.colour).Append(" value=").Append(point.value).Append(" ").Append(timestamp.Ticks * 100L).Append("\n");
            }

            return lines.ToString();
        }
    }
}