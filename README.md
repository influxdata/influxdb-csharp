# InfluxDB .NET Collector [![Build status](https://ci.appveyor.com/api/projects/status/0tqovixkf1e1pqu3/branch/master?svg=true)](https://ci.appveyor.com/project/NicholasBlumhardt/influxdb-lineprotocol/branch/master) [![NuGet Version](http://img.shields.io/nuget/v/InfluxDB.LineProtocol.svg?style=flat)](https://www.nuget.org/packages/InfluxDB.LineProtocol/)

This is a C# implementation of the [InfluxDB](http://influxdb.org) ingestion ['Line Protocol'](https://influxdb.com/docs/v0.9/write_protocols/line.html).

You can use it to write time series data to InfluxDB version 0.9.3+ over HTTP or HTTPS. Two packages are provided:

 * A higher-level metrics-oriented API described in _Getting Started_ below
 * A bare-bones HTTP line protocol client, described in the _Raw Client API_ section

Supporting the full/read API of InfluxDB is an explicit _non-goal_: this package will be kept small so as to have a minimal footprint when used in client applications.

## Getting Started

Install the _InfluxDB.Collector_ NuGet package:

```powershell
Install-Package InfluxDB.Collector -Pre
```

Add `using` statements where needed:

```csharp
using InfluxDB.Collector;
```

Configure a `MetricsCollector`. These can be used directly, or via the static `Metrics` class:

```csharp
Metrics.Collector = new CollectorConfiguration()
    .Tag.With("host", Environment.GetEnvironmentVariable("COMPUTERNAME"))
    .Batch.AtInterval(TimeSpan.FromSeconds(2))
    .WriteTo.InfluxDB("http://192.168.99.100:8086", "data")
    .CreateCollector();
```

Send points using the methods of `MetricsCollector` or `Metrics`:

```csharp
Metrics.Increment("iterations");

Metrics.Write("cpu_time",
    new Dictionary<string, object>
    {
        { "value", process.TotalProcessorTime.TotalMilliseconds },
        { "user", process.UserProcessorTime.TotalMilliseconds }
    });

Metrics.Measure("working_set", process.WorkingSet64);
```

View aggregated metrics in a dashboarding interface such as [Grafana](http://grafana.org).

## Raw Client API

The raw API is a very thin wrapper on InfluxDB's HTTP API, in the _InfluxDB.LineProtocol_ package.

```powershell
Install-Package InfluxDB.LineProtocol -Pre
```

To send points, create a `LineProtocolPayload` containing a batch of `LineProtocolPoint`s. Each point carries the measurement name, at least one value, an optional set of tags and an optional timestamp:

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
var client = new LineProtocolClient("http://my-server:8086", "data");
var influxResult = await client.WriteAsync(payload);
if (!influxResult.Success)
    Console.Error.WriteLine(influxResult.ErrorMessage);
```

## Status

This project is still undergoing some change while in development, but the core functionality is stabilizing. See issues tagged `enhancement` for roadmap items. It's currently targeting .NET 4.5.1 and .NET Core using Visual Studio 2015.
