namespace InfluxDB.Collector.Pipeline
{
    interface IPointEnricher
    {
        void Enrich(PointData pointData);
    }
}
