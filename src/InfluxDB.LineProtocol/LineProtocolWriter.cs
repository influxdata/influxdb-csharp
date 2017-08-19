using System;
using System.Globalization;
using System.IO;

namespace InfluxDB.LineProtocol
{
    public class LineProtocolWriter
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly TextWriter textWriter;

        public WriterPosition Position { get; private set; } = WriterPosition.NothingWritten;

        public LineProtocolWriter()
        {
            this.textWriter = new StringWriter();
        }

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

            switch (Position)
            {
                case WriterPosition.NothingWritten:
                    break;
                case WriterPosition.FieldWritten:
                case WriterPosition.TimestampWritten:
                    textWriter.Write("\n");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            textWriter.Write(EscapeName(name));

            Position = WriterPosition.MeasurementWritten;

            return this;
        }

        public LineProtocolWriter Tag(string name, string value)
        {
            switch (Position)
            {
                case WriterPosition.MeasurementWritten:
                case WriterPosition.TagWritten:
                    textWriter.Write(",");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            textWriter.Write(EscapeName(name));
            textWriter.Write('=');
            textWriter.Write(EscapeName(value));

            return this;
        }

        public LineProtocolWriter Field(string name, float value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            Position = WriterPosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, double value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            Position = WriterPosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, decimal value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            Position = WriterPosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, long value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));
            textWriter.Write('i');

            Position = WriterPosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, string value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write('"');
            textWriter.Write(value.Replace("\"", "\\\""));
            textWriter.Write('"');

            Position = WriterPosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Field(string name, bool value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value ? 't' : 'f');

            Position = WriterPosition.FieldWritten;

            return this;
        }

        public LineProtocolWriter Timestamp(long value)
        {
            switch (Position)
            {
                case WriterPosition.FieldWritten:
                    textWriter.Write(" ");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            Position = WriterPosition.TimestampWritten;

            return this;
        }

        public LineProtocolWriter Timestamp(TimeSpan value)
        {
            return Timestamp(value.Ticks * 100L);
        }

        public LineProtocolWriter Timestamp(DateTime value)
        {
            return Timestamp(value - UnixEpoch);
        }

        public override string ToString()
        {
            return textWriter.ToString();
        }

        private void WriteFieldKey(string name)
        {
            switch (Position)
            {
                case WriterPosition.MeasurementWritten:
                case WriterPosition.TagWritten:
                    textWriter.Write(" ");
                    break;
                case WriterPosition.FieldWritten:
                    textWriter.Write(",");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            textWriter.Write(EscapeName(name));
        }

        public static string EscapeName(string nameOrKey)
        {
            if (nameOrKey == null)
            {
                throw new ArgumentNullException(nameof(nameOrKey));
            }

            return nameOrKey
                .Replace("=", "\\=")
                .Replace(" ", "\\ ")
                .Replace(",", "\\,");
        }

        public enum WriterPosition
        {
            NothingWritten,
            MeasurementWritten,
            TagWritten,
            FieldWritten,
            TimestampWritten
        }
    }
}
