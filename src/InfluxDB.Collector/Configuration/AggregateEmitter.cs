using System;
using System.Collections.Generic;
using InfluxDB.Collector.Pipeline;

namespace InfluxDB.Collector.Configuration
{
    class AggregateEmitter : IPointEmitter
    {
        readonly List<IPointEmitter> _emitters;

        public AggregateEmitter(List<IPointEmitter> emitters)
        {
            if (emitters == null) throw new ArgumentNullException(nameof(emitters));
            _emitters = emitters;
        }

        public void Emit(PointData[] points)
        {
            foreach (var emitter in _emitters)
                emitter.Emit(points);
        }
    }
}