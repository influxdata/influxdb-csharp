# InfluxDB.LineProtocol

This is a C# implementation of the [InfluxDB](http://influxdb.org) ingestion ['Line Protocol'](https://influxdb.com/docs/v0.9/write_protocols/line.html).

You can use it to write time series data to InfluxDB version 0.9.3+ over HTTP or HTTPS.

The initial goal of this package is to create a straightforward and efficient .NET binding to the protocol, with a minimum of syntactic sugar. Once that's complete, a "friendly" metrics collection API may be added on top of the basic functionality. 

Supporting the full/read API of InfluxDB is an explicit _non-goal_: this package will be kept small so as to have a minimal footprint when used in client applications.

## Getting Started

Install the _InfluxDB.LineProtocol_ NuGet package and add `using` statements where needed:

```csharp
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
```

Create a `LineProtocolPayload` containing a batch of `LineProtocolPoint`s. Each point carries the measurement name, at least one value, an optional set of tags and an optional timestamp:

```csharp
var cpuTime = new LineProtocolPoint(
    "working_set",
    new Dictionary<string, object>
    {
        { "value", process.WorkingSet64 },
    },
    new Dictionary<string, string>
    {
        { "host", Environment.GetEnvironmentVariable("COMPUTERNAME") }
    },
    DateTime.UtcNow);

var payload = new LineProtocolPayload();
payload.Add(cpuTime);
// Add more points...
```

(If the timestamp is not specified, the InfluxDB server will assign a timestamp to each point on arrival.)

Write the points to InfluxDB, specifying the server's base URL, database name, and an optional username and password:

```csharp
var influxResult = await client.WriteAsync(payload);
if (!influxResult.Success)
    Console.Error.WriteLine(influxResult.ErrorMessage);
```

## Status

This project is in the early stages of development. It's targeting .NET 4.5.1 and the modern .NET platform using Visual Studio 2015 and the 'DNX' tooling.

Roadmap for anyone keen to help out:

 - [ ] Tests, tests and more tests
 - [ ] Complete support for the parameters accepted to Influx's `/write` endpoint
 - [ ] Possibly a "friendlier" metrics API on top of `LineProtocolClient` and `LineProtocolPayload`
