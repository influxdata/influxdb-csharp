using System;
using InfluxDB.Collector.Diagnostics;
using Xunit;

namespace InfluxDB.LineProtocol.Tests.Collector
{
    public class CollectorLogTests : IDisposable
    {
        public CollectorLogTests()
        {
            CollectorLog.ClearHandlers();
        }

        [Fact]
        public void CanReportWithoutListeners()
        {
            CollectorLog.ReportError("", null);
        }

        [Fact]
        public void SingleListenerGetsInvoked()
        {
            var invocationCount = 0;
            Exception dummyException = new Exception("Bang!");
            const string errorMessage = "Bad things";

            CollectorLog.RegisterErrorHandler((message, exception) =>
            {
                Assert.Equal(errorMessage, message);
                Assert.Equal(dummyException, exception);
                invocationCount++;
            });

            CollectorLog.ReportError(errorMessage, dummyException);
            Assert.Equal(1, invocationCount);
        }

        [Fact]
        public void AllListenersGetInvoked()
        {
            var invocationCount = 0;
            Exception dummyException = new Exception("Bang!");
            const string errorMessage = "Bad things";

            CollectorLog.RegisterErrorHandler((message, exception) =>
            {
                Assert.Equal(errorMessage, message);
                Assert.Equal(dummyException, exception);
                invocationCount++;
            });
            CollectorLog.RegisterErrorHandler((message, exception) => invocationCount++);

            CollectorLog.ReportError(errorMessage, dummyException);
            Assert.Equal(2, invocationCount);
        }

        [Fact]
        public void ListenerCanUnregister()
        {
            var invocationCount = 0;
            using (CollectorLog.RegisterErrorHandler((message, exception) => invocationCount++))
            {
                CollectorLog.ReportError("", null);
            }
            CollectorLog.ReportError("", null);
            Assert.Equal(1, invocationCount);
        }

        [Fact]
        public void EnsureListenerExceptionPropagates()
        {
            CollectorLog.RegisterErrorHandler((message, exception) =>
            {
                throw new Exception("");
            });

            var threw = false;
            try
            {
                CollectorLog.ReportError("", null);
            }
            catch
            {
                threw = true;
            }
            Assert.True(threw, "Excpected an exception thrown by the error handler to propagate to the caller");
        }

        public void Dispose()
        {
            CollectorLog.ClearHandlers();
        }
    }
}
