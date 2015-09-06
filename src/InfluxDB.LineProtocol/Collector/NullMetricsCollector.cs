namespace InfluxDB.LineProtocol.Collector
{
    class NullMetricsCollector : MetricsCollector
    {
        protected override void Emit(PointData[] points)
        {
        }
    }
}
