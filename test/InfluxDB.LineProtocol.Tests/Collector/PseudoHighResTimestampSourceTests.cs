using System;
using Xunit;
using System.Threading.Tasks;
using Xunit.Abstractions;
using InfluxDB.Collector.Util;

namespace InfluxDB.LineProtocol.Tests.Collector.Util
{
    public class PseudoHighResTimeStampSourceTests
    {
        private readonly ITestOutputHelper output;

        public PseudoHighResTimeStampSourceTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CanSupplyHighResTimestamps()
        {
            const int iterations = 100000;

            // Even if we call inside a very tight loop, 
            // we should get better (pseudo) resolution than just DateTime.UtcNow
            long dateTimeCollisions = 0;
            var previousDateTime = DateTime.UtcNow;
            for (var i = 0; i < iterations; i++)
            {
                // Attempt with date time directly
                var current = DateTime.UtcNow;

                if (previousDateTime == current)
                {
                    dateTimeCollisions++;
                }
                previousDateTime = current;
            }
            
            if (0 == dateTimeCollisions)
            {
                // Warn because we do expect that datetime.now has collisions in such a tight loop
                output.WriteLine("Warning! Expected DateTime.UtcNow to have some collisions, but seemed to be high resolution already.");
            }

            // Now attempt to use our pseudo high precision provider that includes a sequence number to ensure that it 
            // never collides
            ITimestampSource target = new PseudoHighResTimestampSource();
            long highResTotalCollisions = 0;
            previousDateTime = target.GetUtcNow();
            for (var i = 0; i < iterations; i++)
            {
                // Attempt with date time directly
                var current = target.GetUtcNow();

                if (previousDateTime == current)
                {
                    highResTotalCollisions++;
                }
                previousDateTime = current;
            }

            Assert.Equal(0, highResTotalCollisions);
            output.WriteLine($"No collisions detected with high resolution source, compared to {dateTimeCollisions} for DateTime.UtcNow.");
        }

        [Fact]
        public void CanSupplyHighResTimestampsInParallel()
        {
            const int iterations = 100000;

            // Even if we call inside a very tight loop, we should get better (pseudo) resolution than just DateTime.UtcNow
            var dateTimeCollisions = 0;
            var previousDateTime = DateTime.UtcNow;
            Parallel.For(0, iterations, (i) => // (int i = 0; i < 100000; i++)
            {
                // Attempt with date time directly
                var current = DateTime.UtcNow;

                if (previousDateTime == current)
                {
                    System.Threading.Interlocked.Increment(ref dateTimeCollisions);
                }
                previousDateTime = current;
            });

            if (0 == dateTimeCollisions)
            {
                // Warn because we do expect that datetime.now has collisions in such a tight loop
                output.WriteLine("Warning! Expected DateTime.UtcNow to have some collisions, but seemed to be high resolution already.");
            }

            // Now attempt to use our pseudo high precision provider that includes a sequence number to ensure that it 
            // never collides
            ITimestampSource target = new PseudoHighResTimestampSource();
            var highResTotalCollisions = 0;
            previousDateTime = target.GetUtcNow();
            Parallel.For(0, iterations, (i) => // (int i = 0; i < 100000; i++)
            {
                // Attempt with date time directly
                var current = target.GetUtcNow();

                if (previousDateTime == current)
                {
                    System.Threading.Interlocked.Increment(ref highResTotalCollisions);
                }
                previousDateTime = current;
            });

            Assert.Equal(0, highResTotalCollisions);
            output.WriteLine($"No collisions detected with high resolution source, compared to {dateTimeCollisions} for DateTime.UtcNow.");
        }

        [Fact]
        public void WillGiveUtcDateTimeKind()
        {
            ITimestampSource target = new PseudoHighResTimestampSource();

            var result = target.GetUtcNow();
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        [Fact]
        public void WillNotDriftTooFarFromUtcNow()
        {
            ITimestampSource target = new PseudoHighResTimestampSource();
            const int MAX_DRIFT_MS = 10;

            // Average over 10000 iterations and get the average drift
            decimal totalDrift = 0;
            const int iterations = 10000;
            for (var i = 0; i < iterations; i++)
            {
                var current = DateTime.UtcNow;
                var result = target.GetUtcNow();

                totalDrift += Convert.ToDecimal((result - current).TotalMilliseconds);
            }
            var averageDrift = totalDrift / iterations;

            if (averageDrift > MAX_DRIFT_MS)
            {
                output.WriteLine($"Expected times were more than {MAX_DRIFT_MS}ms apart. Instead they were {averageDrift}ms apart.");
                Assert.True(false); // Force fail.
            }
            else
            {
                output.WriteLine ($"Total Drift over {iterations} iterations: {totalDrift}ms. Average {averageDrift}ms");
            }
        }
    }
}


