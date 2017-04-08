using System;
using System.Collections.Concurrent;
using InfluxDB.Collector.Util;

namespace InfluxDB.Collector.Diagnostics
{
    public static class CollectorLog
    {
        private static readonly ConcurrentDictionary<Action<string, Exception>, object> ErrorHandlers = new ConcurrentDictionary<Action<string, Exception>, object>();

        /// <summary>
        /// Registers an error handler. Errors reported may not neccessarily always correspond with an exception. That is to say, when the callback is invoked, the exception may be null.
        /// To unregister a handler, simply dispose of the IDisposable returned from this method
        /// The order in which handlers are invoked is not defined
        /// </summary>
        /// <param name="errorHandler">A func accepting a string and an exception. Note that exception may be null when invoked</param>
        /// <returns>An IDispoable which when explicitly disposed, will unnregister the handler</returns>
        public static IDisposable RegisterErrorHandler(Action<string, Exception> errorHandler)
        {
            ErrorHandlers.TryAdd(errorHandler, errorHandler);
            return new DisposableAction(() => ErrorHandlers.TryRemove(errorHandler, out object junk));
        }

        internal static void ReportError(string message, Exception exception)
        {
            foreach (var errorHandler in ErrorHandlers)
            {
                errorHandler.Key(message, exception);
            }
        }

        internal static void ClearHandlers()
        {
            ErrorHandlers.Clear();
        }
    }
}
