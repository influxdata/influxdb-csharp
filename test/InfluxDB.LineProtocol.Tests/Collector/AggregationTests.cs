using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Collector;
using InfluxDB.Collector.Pipeline;
using Xunit;

namespace InfluxDB.LineProtocol.Tests.Collector
{
    public class AggregationTests
    {
        [Fact]
        public async Task IncrementsCanBeSummed()
        {
            var list = new List<PointData>();

            IPointEmitter collector = new CollectorConfiguration()
                .Aggregate.AtInterval(TimeSpan.FromMilliseconds(500))
                .Aggregate.SumIncrements()
                .WriteTo.Emitter(pts => list.AddRange(pts))
                .CreateCollector();

            collector.Emit(new[]
            {
                new PointData("foo",
                    new Dictionary<string, object> { { "count", 1L } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 0)),

                new PointData("foo",
                    new Dictionary<string, object> { { "count", 2L } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 200)),

                new PointData("foo",
                    new Dictionary<string, object> { { "count", 3L } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 300))
            });

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.Equal(1, list.Count);
            Assert.Equal(6L, list[0].Fields["count"]);
            Assert.InRange(list[0].UtcTimestamp.Value.TimeOfDay, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300));
        }

        [Fact]
        public async Task TimesCanBeAveraged()
        {
            var list = new List<PointData>();

            IPointEmitter collector = new CollectorConfiguration()
                .Aggregate.AtInterval(TimeSpan.FromMilliseconds(400))
                .Aggregate.AggregateTimes(Enumerable.Average)
                .WriteTo.Emitter(pts => list.AddRange(pts))
                .CreateCollector();

            collector.Emit(new[]
            {
                new PointData("foo",
                    new Dictionary<string, object> { { "value", TimeSpan.FromSeconds(1) } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 0)),

                new PointData("foo",
                    new Dictionary<string, object> { { "value", TimeSpan.FromSeconds(2) } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 200)),

                new PointData("foo",
                    new Dictionary<string, object> { { "value", TimeSpan.FromSeconds(3) } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 300))
            });

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.Equal(1, list.Count);
            Assert.Equal(TimeSpan.FromSeconds(2), (TimeSpan) list[0].Fields["value"]);
            Assert.InRange(list[0].UtcTimestamp.Value.TimeOfDay, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300));
        }

        [Fact]
        public async Task DifferentTagsArentAggregated()
        {
            var list = new List<PointData>();

            IPointEmitter collector = new CollectorConfiguration()
                .Aggregate.AtInterval(TimeSpan.FromMilliseconds(500))
                .Aggregate.SumIncrements()
                .WriteTo.Emitter(pts => list.AddRange(pts))
                .CreateCollector();

            collector.Emit(new[]
            {
                new PointData("foo",
                    new Dictionary<string, object> { { "count", 1L } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 0)),

                new PointData("foo",
                    new Dictionary<string, object> { { "count", 2L } },
                    new Dictionary<string, string> { { "tag1", "b" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 200)),

                new PointData("foo",
                    new Dictionary<string, object> { { "count", 3L } },
                    new Dictionary<string, string> { { "tag1", "c" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 300))
            });

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.Equal(3, list.Count);
        }

        [Fact]
        public async Task DifferentMeasurementsArentAggregated()
        {
            var list = new List<PointData>();

            IPointEmitter collector = new CollectorConfiguration()
                .Aggregate.AtInterval(TimeSpan.FromMilliseconds(500))
                .Aggregate.SumIncrements()
                .WriteTo.Emitter(pts => list.AddRange(pts))
                .CreateCollector();

            collector.Emit(new[]
            {
                new PointData("foo",
                    new Dictionary<string, object> { { "count", 1L } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 0)),

                new PointData("bar",
                    new Dictionary<string, object> { { "count", 2L } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 200)),

                new PointData("baz",
                    new Dictionary<string, object> { { "count", 3L } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 300))
            });

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.Equal(3, list.Count);
        }

        [Fact]
        public async Task DifferentTimeSpansArentAggregated()
        {
            var list = new List<PointData>();

            IPointEmitter collector = new CollectorConfiguration()
                .Aggregate.AtInterval(TimeSpan.FromMilliseconds(500))
                .Aggregate.SumIncrements()
                .WriteTo.Emitter(pts => list.AddRange(pts))
                .CreateCollector();

            collector.Emit(new[]
            {
                new PointData("foo",
                    new Dictionary<string, object> { { "count", 1L } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 0)),

                new PointData("foo",
                    new Dictionary<string, object> { { "count", 2L } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 700)),

                new PointData("foo",
                    new Dictionary<string, object> { { "count", 3L } },
                    new Dictionary<string, string> { { "tag1", "a" } },
                    new DateTime(2018, 1, 1, 0, 0, 0, 800))
            });

            await Task.Delay(TimeSpan.FromSeconds(1));

            Assert.Equal(2, list.Count);
            Assert.True(list.Any(x => (long) x.Fields["count"] == 1));
            Assert.True(list.Any(x => (long) x.Fields["count"] == 5));
        }
    }
}