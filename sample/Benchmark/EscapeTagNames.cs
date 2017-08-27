using System.IO;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class EscapeTagNames
    {
        private const int N = 500;

        [Params("my_tag", "my tag")]
        public string TagName { get; set; }

        [Benchmark(Baseline = true)]
        public string NoEscaping()
        {
            var writer = new StringWriter();

            for (var i = 0; i < N; i++)
            {
                writer.Write(TagName);
            }

            return writer.ToString();
        }

        [Benchmark]
        public string Replace()
        {
            var writer = new StringWriter();

            for (var i = 0; i < N; i++)
            {
                writer.Write(TagName
                    .Replace("=", "\\=")
                    .Replace(" ", "\\ ")
                    .Replace(",", "\\,"));
            }

            return writer.ToString();
        }

        [Benchmark]
        public string WriteCharOrEscapeString()
        {
            var writer = new StringWriter();

            for (var i = 0; i < N; i++)
            {
                foreach (char c in TagName)
                {
                    switch (c)
                    {
                        case ' ':
                            writer.Write("\\ ");
                            break;
                        case ',':
                            writer.Write("\\,");
                            break;
                        case '=':
                            writer.Write("\\=");
                            break;
                        default:
                            writer.Write(c);
                            break;
                    }
                }
            }

            return writer.ToString();
        }
    }
}