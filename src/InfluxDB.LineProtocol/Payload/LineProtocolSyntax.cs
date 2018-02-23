using System;
using System.Collections.Generic;
using System.Globalization;

namespace InfluxDB.LineProtocol.Payload
{
    class LineProtocolSyntax
    {
        static readonly DateTime Origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static readonly Dictionary<Type, Func<object, string>> Formatters = new Dictionary<Type, Func<object, string>>
        {
            { typeof(sbyte), FormatInteger },
            { typeof(byte), FormatInteger },
            { typeof(short), FormatInteger },
            { typeof(ushort), FormatInteger },
            { typeof(int), FormatInteger },
            { typeof(uint), FormatInteger },
            { typeof(long), FormatInteger },
            { typeof(ulong), FormatInteger },
            { typeof(float), FormatFloat },
            { typeof(double), FormatFloat },
            { typeof(decimal), FormatFloat },
            { typeof(bool), FormatBoolean },
            { typeof(TimeSpan), FormatTimespan }
        };

        public static string EscapeName(string nameOrKey)
        {
            if (nameOrKey == null) throw new ArgumentNullException(nameof(nameOrKey));
            return nameOrKey
                .Replace("=", "\\=")
                .Replace(" ", "\\ ")
                .Replace(",", "\\,");
        }

        public static string FormatValue(object value)
        {
            var v = value ?? "";
            Func<object, string> format;
            if (Formatters.TryGetValue(v.GetType(), out format))
                return format(v);
            return FormatString(v.ToString());
        }

        static string FormatInteger(object i)
        {
            return ((IFormattable)i).ToString(null, CultureInfo.InvariantCulture) + "i";
        }

        static string FormatFloat(object f)
        {
            return ((IFormattable)f).ToString(null, CultureInfo.InvariantCulture);
        }

        static string FormatTimespan(object ts)
        {
            return ((TimeSpan)ts).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        }

        static string FormatBoolean(object b)
        {
            return (bool)b ? "t" : "f";
        }

        static string FormatString(string s)
        {
            return "\"" + s.Replace("\"", "\\\"") + "\"";
        }

        public static string FormatTimestamp(DateTime utcTimestamp, Precision precision)
        {
            TimeSpan ts = utcTimestamp - Origin;
            switch (precision)
            {
                case Precision.Nanoseconds:
                    return (ts.Ticks * 100).ToString(CultureInfo.InvariantCulture);
                case Precision.Microseconds:
                    return (ts.Ticks / 10).ToString(CultureInfo.InvariantCulture);
                case Precision.Milliseconds:
                    return ((long)ts.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
                case Precision.Seconds:
                    return ((long)ts.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                case Precision.Minutes:
                    return ((long)ts.TotalMinutes).ToString(CultureInfo.InvariantCulture);
                case Precision.Hours:
                    return ((long)ts.TotalHours).ToString(CultureInfo.InvariantCulture);
                default:
                    throw new NotSupportedException("Precision is unknown to the formatter.");
            }
        }
    }
}
