using InfluxDB.Collector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

// influx-db command line:
// start with
// # influx
// # show databases
// # CREATE DATABASE {name}
// # DROP DATABASE {name}
// # precision rfc3339
// # use <database>
// # SHOW MEASUREMENTS
// # SHOW MEASUREMENTS WITH MEASUREMENT =~ /v1\..*/ -- all fields from measurements that start with 'v1.' 
// # SHOW SERIES
// # SHOW SERIES [FROM <measurement_name> [WHERE <tag_key>='<tag_value>']]
// # DROP SERIES FROM /v1.*\.end/
// # SHOW TAG KEYS
// # SHOW TAG KEYS FROM "v1.cos"
// # SHOW FIELD KEYS
// # SHOW FIELD KEYS FROM /v1\..*\.sin/   -- all fields from series that start with 'v1.' and end with '.sin'

/*
# influx
docker run --name influx -p 8086:8086 -p 8089:8089/udp -p 8088:8088 -v C:\Docker\Volumes\influxdb\db:/var/lib/influxdb -v C:\Docker\Volumes\influxdb\config\influxdb.conf:/etc/influxdb/influxdb.conf:ro influxdb -config /etc/influxdb/influxdb.conf
docker run -d -p 8083:8083 -p 8086:8086 -p 8089:4444/udp --expose 8083 --expose 8086 --expose 4444 -e UDP_DB="playground" tutum/influxdb

*/
namespace Sample
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Collect().Wait();

            Console.ReadKey();
        }

        async static Task Collect()
        {
            var process = Process.GetCurrentProcess();

            Metrics.Collector = new CollectorConfiguration()
                .Tag.With("host", Environment.GetEnvironmentVariable("COMPUTERNAME"))
                .Tag.With("os", Environment.GetEnvironmentVariable("OS"))
                .Tag.With("process", Path.GetFileName(process.MainModule.FileName))
                .Batch.AtInterval(TimeSpan.FromSeconds(2))
                //.WriteTo.InfluxDB("http://localhost:8086", "data")
                .WriteTo.InfluxDB("udp://localhost:8089", "data")
                .CreateCollector();

            while (true)
            {
                Metrics.Increment("iterations");

                Metrics.Write("cpu_time",
                    new Dictionary<string, object>
                    {
                        { "value", process.TotalProcessorTime.TotalMilliseconds },
                        { "user", process.UserProcessorTime.TotalMilliseconds }
                    });

                Metrics.Measure("working_set", process.WorkingSet64);

                await Task.Delay(1000);
            }
        }
    }
}
