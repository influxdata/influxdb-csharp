using InfluxDB.Collector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace InfluxDb.UdpSupport.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Task t = ListenAsync();
            var process = Process.GetCurrentProcess();

            Metrics.Collector = new CollectorConfiguration()
                .Tag.With("process", Path.GetFileName(process.Id.ToString()))
                .Batch.AtInterval(TimeSpan.FromSeconds(2))
                .WriteTo.InfluxDB("udp://localhost:8089", "data")
                .CreateCollector();

            int i = 0;
            while (true)
            {
                Metrics.Collector.Increment("test", i++ % 10);
                Thread.Sleep(500);
            }
        }

        private static async Task ListenAsync()
        {
            await Task.Delay(1);
            var udpClient = new UdpClient(8999);
            while (true)
            {
                try
                {
                    UdpReceiveResult result = await udpClient.ReceiveAsync();
                    Byte[] receiveBytes = result.Buffer;
                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    IPEndPoint remoteIpEndPoint = result.RemoteEndPoint;

                    // Uses the IPEndPoint object to determine which of these two hosts responded.
                    Console.WriteLine($"This message was sent from {remoteIpEndPoint.Address} on their port number {remoteIpEndPoint.Port}");
                    Console.WriteLine("This is the message you received:");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(returnData);
                    Console.ResetColor();
                    Console.WriteLine("-----------------------------------------------------------------");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
