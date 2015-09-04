namespace InfluxDB.LineProtocol.Client
{
    public struct LineProtocolWriteResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }

        public LineProtocolWriteResult(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
}
