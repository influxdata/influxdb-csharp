using System;
using Xunit;

namespace InfluxDB.LineProtocol.Tests
{
    public class LineProtocolWriterTests
    {
        [Fact]
        public void Write_the_field_value_as_float()
        {
            AssertEqual(
                "mymeas value=1",
                writer => writer.Measurement("mymeas").Field("value", 1f)
            );
        }

        [Fact]
        public void Write_the_field_value_as_int()
        {
            AssertEqual(
                "mymeas value=1i",
                writer => writer.Measurement("mymeas").Field("value", 1)
            );
        }

        [Fact]
        public void Write_the_field_value_as_string()
        {
            AssertEqual(
                "mymeas value=\"stringing along\"",
                writer => writer.Measurement("mymeas").Field("value", "stringing along")
            );
        }

        [Fact]
        public void Write_the_field_value_as_boolean()
        {
            AssertEqual(
                "mymeas value=t",
                writer => writer.Measurement("mymeas").Field("value", true)
            );

            AssertEqual(
                "mymeas value=f",
                writer => writer.Measurement("mymeas").Field("value", false)
            );
        }

        [Fact]
        public void Write_tag_set()
        {
            AssertEqual(
                "weather,location=us-midwest,season=summer temperature=82 1465839830100400200",
                writer => writer.Measurement("weather").Tag("location", "us-midwest").Tag("season", "summer").Field("temperature", 82f).Timestamp(1465839830100400200)
            );
        }

        [Fact]
        public void Write_field_set()
        {
            AssertEqual(
                "weather,location=us-midwest temperature=82,bug_concentration=98 1465839830100400200",
                writer => writer.Measurement("weather").Tag("location", "us-midwest").Field("temperature", 82f).Field("bug_concentration", 98f).Timestamp(1465839830100400200)
            );
        }

        [Fact]
        public void Multiple_lines()
        {
            var lines = new[]
            {
                "h2o_quality,location=coyote_creek,randtag=2 index=49 1441922760",
                "h2o_quality,location=coyote_creek,randtag=3 index=18 1441923120",
                "h2o_quality,location=coyote_creek,randtag=3 index=18 1441923480",
                "h2o_quality,location=coyote_creek,randtag=1 index=51 1441923840"
            };

            AssertEqual(
                string.Join("\n", lines),
                writer =>
                {
                    writer.Measurement("h2o_quality").Tag("location", "coyote_creek").Tag("randtag", "2").Field("index", 49f).Timestamp(1441922760);
                    writer.Measurement("h2o_quality").Tag("location", "coyote_creek").Tag("randtag", "3").Field("index", 18f).Timestamp(1441923120);
                    writer.Measurement("h2o_quality").Tag("location", "coyote_creek").Tag("randtag", "3").Field("index", 18f).Timestamp(1441923480);
                    writer.Measurement("h2o_quality").Tag("location", "coyote_creek").Tag("randtag", "1").Field("index", 51f).Timestamp(1441923840);
                }
            );
        }

        [Fact]
        public void Timestamps_must_be_UTC()
        {
            var writer = new LineProtocolWriter().Measurement("my_measurement").Field("value", 23);

            var ex = Assert.Throws<ArgumentException>(() => writer.Timestamp(DateTime.Now));
            Assert.Equal("Timestamps must be specified as UTC\r\nParameter name: value", ex.Message);
        }

        private void AssertEqual(string expected, Action<LineProtocolWriter> write)
        {
            var writer = new LineProtocolWriter();

            write(writer);

            Assert.Equal(expected, writer.ToString());
        }
    }
}
