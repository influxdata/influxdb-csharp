using System;
using Xunit;

namespace InfluxDB.LineProtocol.Tests
{
    public class LineProtocolWriterInvalidStateTests
    {
        [Fact]
        public void Can_not_write_new_measurment_when_field_not_written()
        {
            var writer = new LineProtocolWriter();

            writer.Measurement("my_measurement");

            var ex = Assert.Throws<InvalidOperationException>(() => writer.Measurement("my_measurement"));
            Assert.Equal("Cannot write new measurement \"my_measurement\" as no field written for current line.", ex.Message);
            Assert.Equal(LineProtocolWriterPosition.MeasurementWritten, ex.Data["Position"]);

            writer.Tag("foo", "bar");

            ex = Assert.Throws<InvalidOperationException>(() => writer.Measurement("my_measurement"));
            Assert.Equal(LineProtocolWriterPosition.TagWritten, ex.Data["Position"]);
        }

        [Fact]
        public void Can_not_write_tag_when_no_measurement_written()
        {
            var writer = new LineProtocolWriter();

            var ex = Assert.Throws<InvalidOperationException>(() => writer.Tag("foo", "bar"));
            Assert.Equal("Cannot write tag \"foo\" as no measurement name written.", ex.Message);
            Assert.Equal(LineProtocolWriterPosition.NothingWritten, ex.Data["Position"]);
        }

        [Fact]
        public void Can_not_write_tag_after_field_written()
        {
            var writer = new LineProtocolWriter();

            writer.Measurement("my_measurement").Field("value", 1);

            var ex = Assert.Throws<InvalidOperationException>(() => writer.Tag("foo", "bar"));
            Assert.Equal("Cannot write tag \"foo\" as field(s) already written for current line.", ex.Message);
            Assert.Equal(LineProtocolWriterPosition.FieldWritten, ex.Data["Position"]);

            writer.Timestamp(123456);

            ex = Assert.Throws<InvalidOperationException>(() => writer.Tag("foo", "bar"));
            Assert.Equal(LineProtocolWriterPosition.TimestampWritten, ex.Data["Position"]);
        }

        [Fact]
        public void Can_not_write_field_when_no_measurement_written()
        {
            var writer = new LineProtocolWriter();

            var ex = Assert.Throws<InvalidOperationException>(() => writer.Field("value", 1.2));
            Assert.Equal("Cannot write field \"value\" as no measurement name written.", ex.Message);
            Assert.Equal(LineProtocolWriterPosition.NothingWritten, ex.Data["Position"]);

            Assert.Throws<InvalidOperationException>(() => writer.Field("value", 3));
            Assert.Throws<InvalidOperationException>(() => writer.Field("value", true));
            Assert.Throws<InvalidOperationException>(() => writer.Field("value", "roar like a lion"));
            Assert.Throws<InvalidOperationException>(() => writer.Field("value", 2.6f));
            Assert.Throws<InvalidOperationException>(() => writer.Field("value", 1000000m));
        }

        [Fact]
        public void Can_not_write_timestamp_when_no_field_written()
        {
            var writer = new LineProtocolWriter();

            var ex = Assert.Throws<InvalidOperationException>(() => writer.Timestamp(123456));
            Assert.Equal("Cannot write timestamp as no measurement name written.", ex.Message);
            Assert.Equal(LineProtocolWriterPosition.NothingWritten, ex.Data["Position"]);

            writer.Measurement("my_measurement");

            ex = Assert.Throws<InvalidOperationException>(() => writer.Timestamp(123456));
            Assert.Equal("Cannot write timestamp as no field written for current measurement.", ex.Message);
            Assert.Equal(LineProtocolWriterPosition.MeasurementWritten, ex.Data["Position"]);

            writer.Tag("foo", "bar");

            ex = Assert.Throws<InvalidOperationException>(() => writer.Timestamp(123456));
            Assert.Equal(LineProtocolWriterPosition.TagWritten, ex.Data["Position"]);

            Assert.Throws<InvalidOperationException>(() => writer.Timestamp(TimeSpan.FromDays(3045)));
            Assert.Throws<InvalidOperationException>(() => writer.Timestamp(DateTime.UtcNow));
            Assert.Throws<InvalidOperationException>(() => writer.Timestamp(DateTimeOffset.Now));
        }
    }
}