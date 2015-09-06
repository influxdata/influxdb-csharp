using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxDB.LineProtocol.Collector
{
    interface IPointEnricher
    {
        void Enrich(PointData pointData);
    }
}
