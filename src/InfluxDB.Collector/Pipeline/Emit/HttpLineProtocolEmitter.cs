using System;
using InfluxDB.Collector.Diagnostics;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.Collector.Pipeline.Emit
{
    class HttpLineProtocolEmitter : IDisposable, IPointEmitter
    {
        readonly LineProtocolClient _client;

        public HttpLineProtocolEmitter(LineProtocolClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _client = client;
        }

        public void Dispose()
        {
             // This needs to ensure outstanding operations have completed
        }

        public void Emit(PointData[] points)
        {
            var payload = new LineProtocolPayload();

            foreach (var point in points)
            {
                payload.Add(new LineProtocolPoint(point.Measurement, point.Fields, point.Tags, point.UtcTimestamp));
            }

            var influxResult = _client.WriteAsync(payload).Result;
            if (!influxResult.Success)
                CollectorLog.WriteLine(influxResult.ErrorMessage);
        }
    }
}
