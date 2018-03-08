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
    public abstract class LineProtocolClientBase : ILineProtocolClient
    {
        protected readonly string _database, _username, _password;

        protected LineProtocolClientBase(Uri serverBaseAddress, string database, string username, string password)
        {
            if (serverBaseAddress == null)
                throw new ArgumentNullException(nameof(serverBaseAddress));
            if (string.IsNullOrEmpty(database))
                throw new ArgumentException("A database must be specified");

            // Overload that allows injecting handler is protected to avoid HttpMessageHandler being part of our public api which would force clients to reference System.Net.Http when using the lib.
            _database = database;
            _username = username;
            _password = password;
        }

        public Task<LineProtocolWriteResult> WriteAsync(LineProtocolPayload payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stringWriter = new StringWriter();

            payload.Format(stringWriter);

            return OnSendAsync(stringWriter.ToString(), Precision.Nanoseconds, cancellationToken);
        }

        public Task<LineProtocolWriteResult> SendAsync(LineProtocolWriter lineProtocolWriter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnSendAsync(lineProtocolWriter.ToString(), lineProtocolWriter.Precision, cancellationToken);
        }

        protected abstract Task<LineProtocolWriteResult> OnSendAsync(
            string payload,
            Precision precision,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
