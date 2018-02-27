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
                .WriteTo.InfluxDB("udp://localhost:8999", "data")
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
