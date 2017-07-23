using System;
using System.Collections.Generic;

namespace InfluxDB.LineProtocol.Payload
{
    public class LineProtocolPayload : ILineProtocolPayload
    {
        readonly List<LineProtocolPoint> _points = new List<LineProtocolPoint>();

        public void Add(LineProtocolPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            _points.Add(point);
        }

        public void Format(LineProtocolWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            foreach (var point in _points)
            {
                point.Format(writer);
            }
        }
    }
}
