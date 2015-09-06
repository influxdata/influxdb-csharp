namespace InfluxDB.LineProtocol.Collector
{
    public abstract class CollectorTagConfiguration
    {
        public abstract CollectorConfiguration With(string key, string value);
    }
}
