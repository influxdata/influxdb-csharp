using Xunit;
using InfluxDB.LineProtocol.Payload;
using System.Collections.Generic;
using System;
using System.IO;

namespace InfluxDB.LineProtocol.Tests
{
    public class LineProtcolPointTests
    {
        [Fact]
        public void CompleteExampleFromDocs()
        {
            // Given in: https://influxdb.com/docs/v0.9/write_protocols/write_syntax.html
            const string expected = "\"measurement\\ with\\ quotes\",tag\\ key\\ with\\ spaces=tag\\,value\\,with\"commas\" field_key\\\\\\\\=\"string field value, only \\\" need be quoted\" 1441756800000000000";

            var point = new LineProtocolPoint(
                "\"measurement with quotes\"",
                new Dictionary<string, object>
                {
                    { "field_key\\\\\\\\", "string field value, only \" need be quoted" }
                },
                new Dictionary<string, string>
                {
                    { "tag key with spaces", "tag,value,with\"commas\"" }
                },
                new DateTime(2015, 9, 9, 0, 0, 0, DateTimeKind.Utc));

            var sw = new StringWriter();
            point.Format(sw);

            Assert.Equal(expected, sw.ToString());
        }
    }
}
