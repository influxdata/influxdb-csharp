using System;
using System.Net.Http;
using InfluxDB.LineProtocol.Client;
using RichardSzalay.MockHttp;

namespace InfluxDB.LineProtocol.Tests.Client
{
    public class MockLineProtocolClient : LineProtocolClient
    {
        public MockLineProtocolClient(string database) : this(new MockHttpMessageHandler(), new Uri("http://localhost:8086"), database)
        {
        }

        private MockLineProtocolClient(MockHttpMessageHandler handler, Uri serverBaseAddress, string database) : base(handler, serverBaseAddress, database, null, null, null)
        {
            Handler = handler;
            BaseAddress = serverBaseAddress;
        }

        public MockHttpMessageHandler Handler { get; }

        public Uri BaseAddress { get; }
    }
}