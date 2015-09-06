using System;

namespace InfluxDB.LineProtocol.Collector
{
    public abstract class CollectorEmitConfiguration
    {
        public abstract CollectorConfiguration InfluxDB(Uri serverBaseAddress, string database, string username = null, string password = null);

        public CollectorConfiguration InfluxDB(string serverBaseAddress, string database, string username = null, string password = null)
        {
            return InfluxDB(new Uri(serverBaseAddress), database, username, password);
        }
    }
}
