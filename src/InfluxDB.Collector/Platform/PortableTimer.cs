using System;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Collector.Diagnostics;

class PortableTimer : IDisposable
{
    readonly object _stateLock = new object();

    readonly Func<CancellationToken, Task> _onTick;
    readonly CancellationTokenSource _cancel = new CancellationTokenSource();

#if THREADING_TIMER
        readonly Timer _timer;
#endif

    bool _running;
    bool _disposed;

    public PortableTimer(Func<CancellationToken, Task> onTick)
    {
        if (onTick == null) throw new ArgumentNullException(nameof(onTick));

        _onTick = onTick;

#if THREADING_TIMER
            _timer = new Timer(_ => OnTick(), null, Timeout.Infinite, Timeout.Infinite);
#endif
    }

    public void Start(TimeSpan interval)
    {
        if (interval < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));

        lock (_stateLock)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PortableTimer));

#if THREADING_TIMER
                _timer.Change(interval, Timeout.InfiniteTimeSpan);
#else
            Task.Delay(interval, _cancel.Token)
                .ContinueWith(
                    _ => OnTick(),
                    CancellationToken.None,
                    TaskContinuationOptions.DenyChildAttach,
                    TaskScheduler.Default);
#endif
        }
    }

    async void OnTick()
    {
        try
        {
            lock (_stateLock)
            {
                if (_disposed)
                {
                    return;
                }

                // There's a little bit of raciness here, but it's needed to support the
                // current API, which allows the tick handler to reenter and set the next interval.

                if (_running)
                {
                    Monitor.Wait(_stateLock);

                    if (_disposed)
                    {
                        return;
                    }
                }

                _running = true;
            }

            if (!_cancel.Token.IsCancellationRequested)
            {
                await _onTick(_cancel.Token);
            }
        }
        catch (OperationCanceledException tcx)
        {
            CollectorLog.ReportError("The timer was canceled during invocation", tcx);
        }
        finally
        {
            lock (_stateLock)
            {
                _running = false;
                Monitor.PulseAll(_stateLock);
            }
        }
    }

    public void Dispose()
    {
        _cancel.Cancel();

        lock (_stateLock)
        {
            if (_disposed)
            {
                return;
            }

            while (_running)
            {
                Monitor.Wait(_stateLock);
            }

#if THREADING_TIMER
                _timer.Dispose();
#endif

            _disposed = true;
        }
    }
}