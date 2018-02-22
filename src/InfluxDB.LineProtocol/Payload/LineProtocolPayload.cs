using System;
using System.Collections.Generic;
using System.IO;

namespace InfluxDB.LineProtocol.Payload
{
    public class LineProtocolPayload
    {
        readonly List<LineProtocolPoint> _points = new List<LineProtocolPoint>();

        public void Add(LineProtocolPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            _points.Add(point);
        }

        public void Format(TextWriter textWriter, Precision precision)
        {
            if (textWriter == null) throw new ArgumentNullException(nameof(textWriter));

            foreach (var point in _points)
            {
                point.Format(textWriter, precision);
                textWriter.Write('\n');
            }
        }
    }
}
