namespace InfluxDB.LineProtocol.Payload
{
    public interface ILineProtocolPayload
    {
        void Format(LineProtocolWriter writer);
    }
}