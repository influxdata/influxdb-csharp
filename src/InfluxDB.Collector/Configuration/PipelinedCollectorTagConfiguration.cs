using System;
using System.Collections.Generic;
using InfluxDB.Collector.Pipeline;
using InfluxDB.Collector.Pipeline.Enrich;

namespace InfluxDB.Collector.Configuration
{
    class PipelinedCollectorTagConfiguration : CollectorTagConfiguration
    {
        readonly CollectorConfiguration _configuration;
        readonly Dictionary<string, string> _tags = new Dictionary<string, string>();

        public PipelinedCollectorTagConfiguration(CollectorConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration;
        }

        public override CollectorConfiguration With(string key, string value)
        {
            _tags[key] = value;
            return _configuration;
        }

        public IPointEnricher CreateEnricher()
        {
            return new DictionaryPointEnricher(_tags);
        }
    }
}
