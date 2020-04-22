using InfluxDB.LineProtocol.Payload;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfluxDB.LineProtocol.Client
{
    public class LineProtocolUdpClient : LineProtocolClientBase
    {
        private readonly UdpClient _udpClient;
        private readonly string _udpHostName;
        private readonly int _udpPort;

        public LineProtocolUdpClient(
                        Uri serverBaseAddress,
                        string database,
                        string username = null,
                        string password = null)
            :base(serverBaseAddress, database, username, password)
        {
            if (serverBaseAddress == null)
                throw new ArgumentNullException(nameof(serverBaseAddress));
            if (string.IsNullOrEmpty(database))
                throw new ArgumentException("A database must be specified");

            _udpHostName = serverBaseAddress.Host;
            _udpPort = serverBaseAddress.Port;
            _udpClient = new UdpClient();
        }

        protected override async Task<LineProtocolWriteResult> OnSendAsync(
                                    string payload,
                                    Precision precision,
                                    CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = Encoding.UTF8.GetBytes(payload);
            int len = await _udpClient.SendAsync(buffer, buffer.Length, _udpHostName, _udpPort);
            return new LineProtocolWriteResult(len == buffer.Length, null);
        }

        protected override void DisposeOfManagedResources()
        {
            _udpClient.Dispose();
        }
    }
}
