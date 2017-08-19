namespace InfluxDB.LineProtocol
{
    public enum LineProtocolWriterPosition
    {
        NothingWritten,
        MeasurementWritten,
        TagWritten,
        FieldWritten,
        TimestampWritten
    }
}