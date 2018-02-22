namespace InfluxDB.LineProtocol
{
    public enum Precision : long
    {
        Nanoseconds = 1,
        Microseconds = 1000,
        Milliseconds = Microseconds * 1000,
        Seconds = Milliseconds * 1000,
        Minutes = Seconds * 60,
        Hours = Minutes * 60
    }
}