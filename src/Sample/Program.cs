using InfluxDB.LineProtocol.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample
{
    public class Program
    {
        public void Main(string[] args)
        {
            var payload = new LineProtocolPayload();

            var measurement = new LineProtocolPoint(
                "cpu",
                new Dictionary<string, object>
                {
                    {"value", 123}
                },
                new Dictionary<string, string>
                {
                    {"host", "nblumhardt-rmbp" }
                },
                DateTime.UtcNow);

            payload.Add(measurement);

            payload.Format(Console.Out);

            Console.Read();
        }
    }
}
