using System;
using System.Globalization;
using System.IO;

namespace InfluxDB.LineProtocol
{
    public class LineProtocolWriter
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly TextWriter textWriter;

        private LinePosition position = LinePosition.NothingWritten;

        public LineProtocolWriter(Precision precision = Precision.Nanoseconds)
        {
            if (!Enum.IsDefined(typeof(Precision), precision))
            {
                throw new ArgumentOutOfRangeException(nameof(precision));
            }

            this.Precision = precision;
            this.textWriter = new StringWriter();
        }

        public Precision Precision { get; }

        public LineProtocolWriter Measurement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }

            switch (position)
            {
                case LinePosition.NothingWritten:
                    break;
                case LinePosition.FieldWritten:
                case LinePosition.TimestampWritten:
                    textWriter.Write("\n");
                    break;
                default:
                    throw InvalidPositionException($"Cannot write new measurement \"{name}\" as no field written for current line.");
            }

            foreach (char c in name)
            {
                switch (c)
                {
                    case ' ':
                        textWriter.Write("\\ ");
                        break;
                    case ',':
                        textWriter.Write("\\,");
                        break;
                    default:
                        textWriter.Write(c);
                        break;
                }
            }

            position = LinePosition.MeasurementWritten;

            return this;
        }

        public LineProtocolWriter Tag(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            switch (position)
            {
                case LinePosition.MeasurementWritten:
                case LinePosition.TagWritten:
                    textWriter.Write(",");
                    break;
                case LinePosition.NothingWritten:
                    throw InvalidPositionException($"Cannot write tag \"{name}\" as no measurement name written.");
                default:
                    throw InvalidPositionException($"Cannot write tag \"{name}\" as field(s) already written for current line.");
            }

            WriteEscapedName(name);
            textWriter.Write('=');
            WriteEscapedName(value);

            position = LinePosition.TagWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, float value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            position = LinePosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, double value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            position = LinePosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, decimal value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            position = LinePosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, long value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));
            textWriter.Write('i');

            position = LinePosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, string value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write('"');
            textWriter.Write(value.Replace("\"", "\\\""));
            textWriter.Write('"');

            position = LinePosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, bool value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value ? 't' : 'f');

            position = LinePosition.FieldWritten;

            return this;
        }

        public void Timestamp(long value)
        {
            switch (position)
            {
                case LinePosition.FieldWritten:
                    textWriter.Write(" ");
                    break;
                case LinePosition.NothingWritten:
                    throw InvalidPositionException("Cannot write timestamp as no measurement name written.");
                default:
                    throw InvalidPositionException("Cannot write timestamp as no field written for current measurement.");
            }

            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            position = LinePosition.TimestampWritten;
        }

        public void Timestamp(TimeSpan value, PrecisionResolutionStrategies resolutionStrategy = PrecisionResolutionStrategies.Error)
        {
            var nanoseconds = value.Ticks * 100;

            var nanoSecondsAbovePrecision = value.Ticks * 100 % (long)Precision;

            if (nanoSecondsAbovePrecision != 0)
            {
                switch (resolutionStrategy)
                {
                    case PrecisionResolutionStrategies.Error:
                        throw new ArgumentOutOfRangeException(nameof(value));
                    case PrecisionResolutionStrategies.Floor:
                        nanoseconds -= nanoSecondsAbovePrecision;
                        break;
                    case PrecisionResolutionStrategies.Ceiling:
                        nanoseconds += (long)Precision - nanoSecondsAbovePrecision;
                        break;
                    case PrecisionResolutionStrategies.Round:
                        if (nanoSecondsAbovePrecision < (long)Precision / 2)
                        {
                            Timestamp(value, PrecisionResolutionStrategies.Floor);
                        }
                        else
                        {
                            Timestamp(value, PrecisionResolutionStrategies.Ceiling);
                        }
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }

            Timestamp(nanoseconds / (long)Precision);
        }

        public void Timestamp(DateTimeOffset value, PrecisionResolutionStrategies resolutionStrategy = PrecisionResolutionStrategies.Error)
        {
            Timestamp(value.UtcDateTime);
        }

        public void Timestamp(DateTime value)
        {
            if (value != null && value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Timestamps must be specified as UTC", nameof(value));
            }

            Timestamp(value - UnixEpoch);
        }

        public override string ToString()
        {
            return textWriter.ToString();
        }

        private void WriteFieldKey(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            switch (position)
            {
                case LinePosition.MeasurementWritten:
                case LinePosition.TagWritten:
                    textWriter.Write(" ");
                    break;
                case LinePosition.FieldWritten:
                    textWriter.Write(",");
                    break;
                default:
                    throw InvalidPositionException($"Cannot write field \"{name}\" as no measurement name written.");
            }

            WriteEscapedName(name);
        }

        private void WriteEscapedName(string name)
        {
            foreach (char c in name)
            {
                switch (c)
                {
                    case ' ':
                        textWriter.Write("\\ ");
                        break;
                    case ',':
                        textWriter.Write("\\,");
                        break;
                    case '=':
                        textWriter.Write("\\=");
                        break;
                    default:
                        textWriter.Write(c);
                        break;
                }
            }
        }

        private InvalidOperationException InvalidPositionException(string message)
        {
            // We don't need an custom exceptions as there should be no need for a dev to catch this condition. They are not using the api right so how can the write code to recover.
            // We can make the current writer position available for better diagnostics then logged.
            return new InvalidOperationException(message)
            {
                Data =
                {
                    { "Position", position.ToString() }
                }
            };
        }

        enum LinePosition
        {
            NothingWritten,
            MeasurementWritten,
            TagWritten,
            FieldWritten,
            TimestampWritten
        }
    }
}
