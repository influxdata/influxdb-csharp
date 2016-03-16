using System.Collections.Generic;
using System.Linq;
using InfluxDB.Collector;
using InfluxDB.Collector.Pipeline;
using Xunit;
using System;
using System.Threading.Tasks;

namespace InfluxDB.LineProtocol.Tests.Collector
{
    public class MetricsCollectorTests
    {
        [Fact]
        public void CollectorsCanBeCreated()
        {
            var collector = new CollectorConfiguration()
                .CreateCollector();

            Assert.NotNull(collector);
        }

        [Fact]
        public void SpecializedCollectorsCanBeCreated()
        {
            var points = new List<PointData>();

            var collector = new CollectorConfiguration()
                .WriteTo.Emitter(pts => points.AddRange(pts))
                .CreateCollector();

            var specialized = collector
                .Specialize()
                .Tag.With("test", "42")
                .CreateCollector();

            specialized.Increment("m");

            var point = points.Single();
            Assert.Equal("42", point.Tags.Single().Value);

            Assert.NotNull(specialized);
        }

        [Fact]
        public void CollectorCanBeDisposedWhileTimerIsWaiting()
        {
            var written = new TaskCompletionSource<object>();

            var collector = new CollectorConfiguration()
                .Batch.AtInterval(TimeSpan.FromDays(1))
                .WriteTo.Emitter(_ => written.SetResult(null))
                .CreateCollector();

            collector.Increment("m");
            written.Task.Wait();

            collector.Dispose();
        }
    }
}
