using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Sample
{
    public class Program
    {
        public void Main(string[] args)
        {
            Collect().Wait();
        }

        async Task Collect()
        {
            var client = new LineProtocolClient(new Uri("http://192.168.99.100:8086"), "data");

            var process = Process.GetCurrentProcess();

            var tags = new Dictionary<string, string>
                {
                    { "host", Environment.GetEnvironmentVariable("COMPUTERNAME") },
                    { "os", Environment.GetEnvironmentVariable("OS") },
                    { "process", Path.GetFileName(process.MainModule.FileName) }
                };

            while (true)
            {
                var now = DateTime.UtcNow;

                var payload = new LineProtocolPayload();

                payload.Add(new LineProtocolPoint(
                    "cpu_time",
                    new Dictionary<string, object>
                    {
                        { "value", process.TotalProcessorTime.TotalMilliseconds },
                        { "user", process.UserProcessorTime.TotalMilliseconds }
                    },
                    tags,
                    now));

                payload.Add(new LineProtocolPoint(
                    "working_set",
                    new Dictionary<string, object>
                    {
                        { "value", process.WorkingSet64 },
                    },
                    tags,
                    now));

                var influxResult = await client.WriteAsync(payload);
                if (!influxResult.Success)
                    Console.Error.WriteLine(influxResult.ErrorMessage);

                await Task.Delay(1000);
            }
        }
    }
}
