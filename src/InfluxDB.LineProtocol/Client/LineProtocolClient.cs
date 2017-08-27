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
        readonly string _database, _username, _password;

        public LineProtocolClient(Uri serverBaseAddress, string database, string username = null, string password = null)
            : this(new HttpClientHandler(), serverBaseAddress, database, username, password)
        {
        }

        protected LineProtocolClient(HttpMessageHandler handler, Uri serverBaseAddress, string database, string username, string password)
        {
            if (serverBaseAddress == null) throw new ArgumentNullException(nameof(serverBaseAddress));
            if (string.IsNullOrEmpty(database)) throw new ArgumentException("A database must be specified");

            // Overload that allows injecting handler is protected to avoid HttpMessageHandler being part of our public api which would force clients to reference System.Net.Http when using the lib.
            _httpClient = new HttpClient(handler) { BaseAddress = serverBaseAddress };
            _database = database;
            _username = username;
            _password = password;
        }

        public Task<LineProtocolWriteResult> WriteAsync(LineProtocolPayload payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            var writer = new StringWriter();

            payload.Format(writer);

            return SendAsync(writer.ToString(), cancellationToken);
        }

        public Task<LineProtocolWriteResult> SendAsync(LineProtocolWriter writer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync(writer.ToString(), cancellationToken);
        }

        private async Task<LineProtocolWriteResult> SendAsync(string payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            var endpoint = $"write?db={Uri.EscapeDataString(_database)}";
            if (!string.IsNullOrEmpty(_username))
                endpoint += $"&u={Uri.EscapeDataString(_username)}&p={Uri.EscapeDataString(_password)}";

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
