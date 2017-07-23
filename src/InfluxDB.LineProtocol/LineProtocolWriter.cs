using System;
using System.Globalization;
using System.IO;
using InfluxDB.LineProtocol.Payload;

namespace InfluxDB.LineProtocol
{
    public class LineProtocolWriter
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly TextWriter textWriter;

        public WriterPosition Position { get; private set; } = WriterPosition.StartOfLine;

        public LineProtocolWriter() : this(new StringWriter())
        {
        }

        internal LineProtocolWriter(TextWriter textWriter)
        {
            this.textWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
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
                case WriterPosition.StartOfLine:
                    break;
                case WriterPosition.FieldWriten:
                case WriterPosition.TimestampWriten:
                    textWriter.Write("\n");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            textWriter.Write(LineProtocolSyntax.EscapeName(name));

            Position = WriterPosition.MeasurementWriten;

            return this;
        }

        public LineProtocolWriter Tag(string name, string value)
        {
            switch (Position)
            {
                case WriterPosition.MeasurementWriten:
                case WriterPosition.TagWriten:
                    textWriter.Write(",");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            textWriter.Write(LineProtocolSyntax.EscapeName(name));
            textWriter.Write('=');
            textWriter.Write(LineProtocolSyntax.EscapeName(value));

            return this;
        }

        public LineProtocolWriter Field(string name, float value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            Position = WriterPosition.FieldWriten;

            return this;
        }

        public LineProtocolWriter Field(string name, double value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            Position = WriterPosition.FieldWriten;

            return this;
        }

        public LineProtocolWriter Field(string name, decimal value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

            Position = WriterPosition.FieldWriten;

            return this;
        }

        public LineProtocolWriter Field(string name, long value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));
            textWriter.Write('i');

            Position = WriterPosition.FieldWriten;

            return this;
        }

        public LineProtocolWriter Field(string name, string value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write('"');
            textWriter.Write(value.Replace("\"", "\\\""));
            textWriter.Write('"');

            Position = WriterPosition.FieldWriten;

            return this;
        }

        public LineProtocolWriter Field(string name, bool value)
        {
            WriteFieldKey(name);
            textWriter.Write('=');
            textWriter.Write(value ? 't' : 'f');

            Position = WriterPosition.FieldWriten;

            return this;
        }

        public LineProtocolWriter Timestamp(long value)
        {
            switch (Position)
            {
                case WriterPosition.FieldWriten:
                    textWriter.Write(" ");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            textWriter.Write(value.ToString(CultureInfo.InvariantCulture));

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
            if (textWriter is StringWriter stringWriter)
            {
                return stringWriter.GetStringBuilder().ToString();
            }

            return $"{GetType().Name} at postion {this.Position}";
        }

        private void WriteFieldKey(string name)
        {
            switch (Position)
            {
                case WriterPosition.MeasurementWriten:
                case WriterPosition.TagWriten:
                    textWriter.Write(" ");
                    break;
                case WriterPosition.FieldWriten:
                    textWriter.Write(",");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            textWriter.Write(LineProtocolSyntax.EscapeName(name));
        }

        public enum WriterPosition
        {
            StartOfLine,
            MeasurementWriten,
            TagWriten,
            FieldWriten,
            TimestampWriten
        }
    }
}
