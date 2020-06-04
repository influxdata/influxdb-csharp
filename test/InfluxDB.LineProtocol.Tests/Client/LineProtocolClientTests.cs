using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Payload;
using RichardSzalay.MockHttp;
using Xunit;

namespace InfluxDB.LineProtocol.Tests.Client
{
    public class LineProtocolClientTests
    {
        [Fact]
        public async Task Sending_uncompressed_data()
        {
            var client = new MockLineProtocolClient("foo", false);

            var payload = new LineProtocolPayload();

            payload.Add(new LineProtocolPoint("bar", new Dictionary<string, object> { { "baz", 42 } }));

            client.Handler
                .Expect($"{client.BaseAddress}write?db=foo")
                .WithContent("bar baz=42i\n")
                .Respond(HttpStatusCode.NoContent);

            var result = await client.WriteAsync(payload);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task Sending_compressed_data()
        {
            var client = new MockLineProtocolClient("foo", true);

            var payload = new LineProtocolPayload();

            payload.Add(new LineProtocolPoint("bar", new Dictionary<string, object> { { "baz", 42 } }));

            client.Handler
                .Expect($"{client.BaseAddress}write?db=foo")
                .WithHeaders("Content-Encoding", "gzip")
                .With(req =>
                {
                    var expected = "bar baz=42i\n";
                    var actual = Gunzip(req.Content.ReadAsStreamAsync().Result);

                    return expected == actual;
                })
                .Respond(HttpStatusCode.NoContent);

            var result = await client.WriteAsync(payload);

            Assert.True(result.Success);
        }

        private string Gunzip(Stream compressed)
        {
            using (var decompressed = new MemoryStream())
            using (var gunzip = new GZipStream(compressed, CompressionMode.Decompress))
            {
                gunzip.CopyTo(decompressed);
                return Encoding.UTF8.GetString(decompressed.ToArray());
            }
        }
    }
}
