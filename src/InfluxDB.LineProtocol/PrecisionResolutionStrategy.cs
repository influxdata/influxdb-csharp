namespace InfluxDB.LineProtocol
{
    public enum PrecisionResolutionStrategy
    {
        Undefined = 0,
        Error,
        Round,
        Floor,
        Ceiling
    }
}