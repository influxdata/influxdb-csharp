using InfluxDB.LineProtocol.Payload;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InfluxDB.LineProtocol.Client
{
    public class LineProtocolClient
    {
        readonly HttpClient _httpClient;
        readonly string _database, _retentionPolicy, _username, _password;

        public LineProtocolClient(Uri serverBaseAddress, string database, string retentionPolicy = null, string username = null, string password = null)
            : this(new HttpClientHandler(), serverBaseAddress, database, retentionPolicy, username, password)
        {
        }

        protected LineProtocolClient(HttpMessageHandler handler, Uri serverBaseAddress, string database, string retentionPolicy, string username, string password)
        {
            if (serverBaseAddress == null) throw new ArgumentNullException(nameof(serverBaseAddress));
            if (string.IsNullOrEmpty(database)) throw new ArgumentException("A database must be specified");

            // Overload that allows injecting handler is protected to avoid HttpMessageHandler being part of our public api which would force clients to reference System.Net.Http when using the lib.
            _httpClient = new HttpClient(handler) { BaseAddress = serverBaseAddress };
            _database = database;
            _retentionPolicy = retentionPolicy;
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
            if (!string.IsNullOrWhiteSpace(_retentionPolicy))
                endpoint += $"&rp={Uri.EscapeDataString(_retentionPolicy)}";
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
            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return new LineProtocolWriteResult(true, null);
            }

            var body = string.Empty;

            if (response.Content != null)
            {
                body = await response.Content.ReadAsStringAsync();
            }

            return new LineProtocolWriteResult(false, $"{response.StatusCode} {response.ReasonPhrase} {body}");
        }
    }
}
