using System;
using System.IO;

namespace InfluxDB.LineProtocol.Collector
{
    public static class CollectorLog
    {
        public static TextWriter Out { get; set; } = Console.Error;

        public static void WriteLine(string format, params object[] args)
        {
            var writer = Out;
            if (writer != null)
                writer.WriteLine(format, args);
        }
    }
}
