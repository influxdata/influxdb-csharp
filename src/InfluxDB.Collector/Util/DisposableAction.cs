using System;
using System.Threading;

namespace InfluxDB.Collector.Util
{
    internal class DisposableAction : IDisposable
    {
        private readonly Action _disposeAction;
        private int _disposeCount;

        public DisposableAction(Action disposeAction)
        {
            if (disposeAction == null) throw new ArgumentNullException(nameof(disposeAction));
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            // Only dispose on first call
            if (Interlocked.Increment(ref _disposeCount) == 1)
                _disposeAction();
        }
    }
}
