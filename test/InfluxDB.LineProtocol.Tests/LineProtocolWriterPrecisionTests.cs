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

        [Theory]
        [InlineData(Precision.Microseconds)]
        [InlineData(Precision.Milliseconds)]
        [InlineData(Precision.Seconds)]
        [InlineData(Precision.Hours)]
        public void Can_floor_if_wrong_precision_used(Precision precision)
        {
            var writer = new LineProtocolWriter(precision);

            var timestamp = TimeSpan.FromTicks(1);

            writer.Measurement("foo").Field("bar", 1f).Timestamp(timestamp, PrecisionResolutionStrategy.Floor);

            Assert.Equal("foo bar=1 0", writer.ToString());
        }

        [Theory]
        [InlineData(Precision.Microseconds)]
        [InlineData(Precision.Milliseconds)]
        [InlineData(Precision.Seconds)]
        [InlineData(Precision.Hours)]
        public void Can_ceiling_if_wrong_precision_used(Precision precision)
        {
            var writer = new LineProtocolWriter(precision);

            var timestamp = TimeSpan.FromTicks(1);

            writer.Measurement("foo").Field("bar", 1f).Timestamp(timestamp, PrecisionResolutionStrategy.Ceiling);

            Assert.Equal("foo bar=1 1", writer.ToString());
        }

        [Theory]
        [InlineData(Precision.Microseconds)]
        [InlineData(Precision.Milliseconds)]
        [InlineData(Precision.Seconds)]
        [InlineData(Precision.Hours)]
        public void Can_round_if_wrong_precision_used(Precision precision)
        {
            var writer = new LineProtocolWriter(precision);

            writer.Measurement("foo").Field("bar", true).Timestamp(TimeSpan.FromTicks(1), PrecisionResolutionStrategy.Round);
            writer.Measurement("foo").Field("bar", true).Timestamp(TimeSpan.FromTicks(((long)precision / 100) - 1), PrecisionResolutionStrategy.Round);

            Assert.Equal("foo bar=t 0\nfoo bar=t 1", writer.ToString());
        }

        [Fact]
        public void Can_define_resolution_strategy_when_creating_the_writer()
        {
            var writer = new LineProtocolWriter(Precision.Seconds, PrecisionResolutionStrategy.Round);

            writer.Measurement("foo").Field("bar", true).Timestamp(TimeSpan.FromMilliseconds(499));
            writer.Measurement("foo").Field("bar", true).Timestamp(TimeSpan.FromMilliseconds(500));

            Assert.Equal("foo bar=t 0\nfoo bar=t 1", writer.ToString());
        }

        [Fact]
        public void Can_override_resolution_strategy_when_writing_point()
        {
            var writer = new LineProtocolWriter(Precision.Seconds, PrecisionResolutionStrategy.Round);

            writer.Measurement("foo").Field("bar", true).Timestamp(TimeSpan.FromMilliseconds(700));
            writer.Measurement("foo").Field("bar", true).Timestamp(TimeSpan.FromMilliseconds(700), PrecisionResolutionStrategy.Floor);

            Assert.Equal("foo bar=t 1\nfoo bar=t 0", writer.ToString());
        }
    }
}