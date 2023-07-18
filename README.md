The v1 client libraries for InfluxDB were typically developed and maintained by
community members. For InfluxDB 3.0 users, this library is succeeded by the
lightweight [v3 client library](https://github.com/InfluxCommunity/influxdb3-csharp).

If there are still users of this v1 client library, and they or somebody else
are willing to keep them updated with security fixes at a minimum please reach
out on the [Community Forums](https://community.influxdata.com/) or
[InfluxData Slack](https://influxdata.com/slack).

# InfluxDB .NET Collector

[![Build status](https://img.shields.io/appveyor/build/influx/influxdb-csharp/dev)](https://ci.appveyor.com/project/influx/influxdb-csharp/) 
[![NuGet Version](http://img.shields.io/nuget/v/InfluxDB.LineProtocol.svg?style=flat)](https://www.nuget.org/packages/InfluxDB.LineProtocol/)
[![License](https://img.shields.io/github/license/influxdata/influxdb-csharp.svg)](https://github.com/influxdata/influxdb-csharp/blob/master/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues-raw/influxdata/influxdb-csharp.svg)](https://github.com/influxdata/influxdb-csharp/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr-raw/influxdata/influxdb-csharp.svg)](https://github.com/influxdata/influxdb-csharp/pulls)
[![Slack Status](https://img.shields.io/badge/slack-join_chat-white.svg?logo=slack&style=social)](https://www.influxdata.com/slack)

### Note: This library is for use with InfluxDB 1.x. For connecting to InfluxDB 2.x instances, please use the [influxdb-client-csharp](https://github.com/influxdata/influxdb-client-csharp) client.

This is a C# implementation of the [InfluxDB](http://influxdb.org) ingestion ['Line Protocol'](https://docs.influxdata.com/influxdb/latest/write_protocols/line_protocol_tutorial/).

You can use it to write time series data to InfluxDB version 0.9.3+ over HTTP or HTTPS. Two packages are provided:

 * A higher-level metrics-oriented API described in _Getting Started_ below
 * A bare-bones HTTP line protocol client, described in the _Raw Client API_ section

Supporting the full/read API of InfluxDB is an explicit _non-goal_: this package will be kept small so as to have a minimal footprint when used in client applications.

## Getting Started

Install the _InfluxDB.Collector_ NuGet package:

```powershell
Install-Package InfluxDB.Collector
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

View aggregated metrics in a dashboarding interface such as [Chronograf](https://www.influxdata.com/time-series-platform/chronograf/) or [Grafana](http://grafana.org).

## Raw Client API

The raw API is a very thin wrapper on InfluxDB's HTTP API, in the _InfluxDB.LineProtocol_ package.

```powershell
Install-Package InfluxDB.LineProtocol
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
var client = new LineProtocolClient(new Uri("http://my-server:8086"), "data");
var influxResult = await client.WriteAsync(payload);
if (!influxResult.Success)
    Console.Error.WriteLine(influxResult.ErrorMessage);
```

## Diagnostics

The collector will not throw exceptions when communication errors occur. To be notified of metric collection issues, register an error handler:

```csharp
CollectorLog.RegisterErrorHandler((message, exception) =>
{
    Console.WriteLine($"{message}: {exception}");
});
```

## Status

This project is still undergoing some change while in development, but the core functionality is stabilizing. See issues tagged `enhancement` for roadmap items. It's currently targeting .NET 4.5.1 and .NET Core using Visual Studio 2017.
