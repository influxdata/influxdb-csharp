using System;
using Xunit;

namespace InfluxDB.LineProtocol.Tests
{
    public class LineProtocolWriterPrecisionTests
    {
        [Theory]
        [InlineData(Precision.Nanoseconds, 1500832764070165800, "1500832764070165800")]
        [InlineData(Precision.Microseconds, 1500832764070165000, "1500832764070165")]
        [InlineData(Precision.Milliseconds, 1500832764070000000, "1500832764070")]
        [InlineData(Precision.Seconds, 1500832764000000000, "1500832764")]
        [InlineData(Precision.Hours, 1500829200000000000, "416897")]
        public void Will_write_timestamps_using_precision_of_writer(Precision precision, long nanoseconds, string expectedTimestamp)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var timestamp = unixEpoch.AddTicks(nanoseconds / 100); // .net tick is 100 nanoseconds.

            var writer = new LineProtocolWriter(precision);

            writer.Measurement("foo").Field("bar", 1f).Timestamp(timestamp);

            Assert.Equal($"foo bar=1 {expectedTimestamp}", writer.ToString());
        }

        [Theory]
        [InlineData(Precision.Microseconds)]
        [InlineData(Precision.Milliseconds)]
        [InlineData(Precision.Seconds)]
        [InlineData(Precision.Hours)]
        public void Will_throw_if_wrong_precision_used(Precision precision)
        {
            var writer = new LineProtocolWriter(precision);

            var timestamp = TimeSpan.FromTicks(1);

            writer.Measurement("foo").Field("bar", 1f);

            Assert.Throws<ArgumentOutOfRangeException>(() => writer.Timestamp(timestamp));
        }
    }
}