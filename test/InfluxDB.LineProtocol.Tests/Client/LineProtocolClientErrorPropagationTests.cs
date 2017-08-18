using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Payload;
using RichardSzalay.MockHttp;
using Xunit;

namespace InfluxDB.LineProtocol.Tests.Client
{
    public class LineProtocolClientErrorPropagationTests
    {
        /// <summary>
        /// Example taken from https://docs.influxdata.com/influxdb/v1.3/guides/writing_data/#http-response-summary
        /// </summary>
        [Fact]
        public async Task Writing_a_float_to_a_field_that_previously_accepted_booleans()
        {
            var client = new MockLineProtocolClient("hamlet");

            var payload = new LineProtocolPayload();

            payload.Add(new LineProtocolPoint(
                "tobeornottobe",
                new Dictionary<string, object>
                {
                    {"booleanonly", true}
                }
            ));

            payload.Add(new LineProtocolPoint(
                "tobeornottobe",
                new Dictionary<string, object>
                {
                    {"booleanonly", 5f}
                }
            ));

            client.Handler.Expect($"{client.BaseAddress}write?db=hamlet")
                .Respond(HttpStatusCode.BadRequest, "application/json", "{\"error\":\"field type conflict: input field \\\"booleanonly\\\" on measurement \\\"tobeornottobe\\\" is type float, already exists as type boolean dropped=1\"}");

            var result = await client.WriteAsync(payload);

            Assert.False(result.Success);
            Assert.Equal("BadRequest Bad Request {\"error\":\"field type conflict: input field \\\"booleanonly\\\" on measurement \\\"tobeornottobe\\\" is type float, already exists as type boolean dropped=1\"}", result.ErrorMessage);
        }

        /// <summary>
        /// Example taken from https://docs.influxdata.com/influxdb/v1.3/guides/writing_data/#http-response-summary
        /// </summary>
        [Fact]
        public async Task Writing_a_point_to_a_database_that_does_not_exist()
        {
            var client = new MockLineProtocolClient("atlantis");

            var payload = new LineProtocolPayload();

            payload.Add(new LineProtocolPoint(
                "liters",
                new Dictionary<string, object>
                {
                    {"value", 10}
                }
            ));

            client.Handler.Expect($"{client.BaseAddress}write?db=atlantis")
                .Respond(HttpStatusCode.NotFound, "application/json", "{\"error\":\"database not found: \\\"atlantis\\\"\"}");

            var result = await client.WriteAsync(payload);

            Assert.False(result.Success);
            Assert.Equal("NotFound Not Found {\"error\":\"database not found: \\\"atlantis\\\"\"}", result.ErrorMessage);
        }
    }
}
