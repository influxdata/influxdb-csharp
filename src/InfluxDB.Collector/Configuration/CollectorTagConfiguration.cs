namespace InfluxDB.Collector.Configuration
{
    public abstract class CollectorTagConfiguration
    {
        public abstract CollectorConfiguration With(string key, string value);
    }
}
