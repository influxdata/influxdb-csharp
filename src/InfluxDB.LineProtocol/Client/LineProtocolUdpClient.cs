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
    public class LineProtocolUdpClient : ILineProtocolClient
    {
        private readonly UdpClient _udpClient;
        private readonly string _udpHostName;
        private readonly int _udpPort;
        private readonly string _database, _username, _password;

        public LineProtocolUdpClient(
            Uri serverBaseAddress,
            string database,
            string username = null,
            string password = null)
        {
            if (serverBaseAddress == null)
                throw new ArgumentNullException(nameof(serverBaseAddress));
            if (string.IsNullOrEmpty(database))
                throw new ArgumentException("A database must be specified");

            _udpHostName = serverBaseAddress.Host;
            _udpPort = serverBaseAddress.Port;
            _udpClient = new UdpClient();
            _database = database;
            _username = username;
            _password = password;
        }

        public Task<LineProtocolWriteResult> WriteAsync(LineProtocolPayload payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stringWriter = new StringWriter();

            payload.Format(stringWriter);

            return SendAsync(stringWriter.ToString(), Precision.Nanoseconds, cancellationToken);
        }

        public Task<LineProtocolWriteResult> SendAsync(LineProtocolWriter lineProtocolWriter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync(lineProtocolWriter.ToString(), lineProtocolWriter.Precision, cancellationToken);
        }

        private async Task<LineProtocolWriteResult> SendAsync(string payload, Precision precision, CancellationToken cancellationToken = default(CancellationToken))
        {
            var endpoint = $"write?db={Uri.EscapeDataString(_database)}";
            if (!string.IsNullOrEmpty(_username))
                endpoint += $"&u={Uri.EscapeDataString(_username)}&p={Uri.EscapeDataString(_password)}";

            switch (precision)
            {
                case Precision.Microseconds:
                    endpoint += "&precision=u";
                    break;
                case Precision.Milliseconds:
                    endpoint += "&precision=ms";
                    break;
                case Precision.Seconds:
                    endpoint += "&precision=s";
                    break;
                case Precision.Minutes:
                    endpoint += "&precision=m";
                    break;
                case Precision.Hours:
                    endpoint += "&precision=h";
                    break;
            }

            var content = new StringContent(payload, Encoding.UTF8);

            var buffer = Encoding.UTF8.GetBytes(payload);
            int len = await _udpClient.SendAsync(buffer, buffer.Length, _udpHostName, _udpPort);
            return new LineProtocolWriteResult(len == buffer.Length, null);
        }
    }
}
