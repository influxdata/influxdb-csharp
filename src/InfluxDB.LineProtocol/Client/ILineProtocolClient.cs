using System;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.LineProtocol.Client
{
    public interface ILineProtocolClient : IDisposable
    {
        Task<LineProtocolWriteResult> SendAsync(
            LineProtocolWriter lineProtocolWriter,
            CancellationToken cancellationToken = default(CancellationToken));
        Task<LineProtocolWriteResult> WriteAsync(
            LineProtocolPayload payload, 
            CancellationToken cancellationToken = default(CancellationToken));
    }
}