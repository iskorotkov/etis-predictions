using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtisPredictions.Preprocessor.Statistics;

namespace EtisPredictions.Preprocessor.Passes
{
    public class StatEnhancer : IPass
    {
        private readonly int _statsSize;
        private readonly StatAnalyzer _analyzer;
        private readonly Layout _layout;
        private readonly int _sequenceLength;

        private struct Stat
        {
            public string Header { get; }
            public Func<StatValues, double> Expr { get; }

            public Stat(string header, Func<StatValues, double> expr)
            {
                Header = header;
                Expr = expr;
            }
        }

        private static readonly Stat[] StatsHeaders =
        {
            new Stat("Min", x => x.Min),
            new Stat("Max", x => x.Max),
            new Stat("Mean", x => x.Mean),
            new Stat("Median", x => x.Median),
            new Stat("Variance", x => x.Variance),
            new Stat("Percentile 10", x => x.Percentile10),
            new Stat("Percentile 25", x => x.Percentile25),
            new Stat("Percentile 75", x => x.Percentile75),
            new Stat("Percentile 90", x => x.Percentile90)
        };

        public StatEnhancer(Layout layout, int sequenceLength = 30, double defaultValue = 80.0)
        {
            _statsSize = StatsHeaders.Length;
            _layout = layout;
            _sequenceLength = sequenceLength;
            _analyzer = new StatAnalyzer(defaultValue);
        }

        public async Task<string> Do(string source, string destination, Encoding encoding)
        {
            using var reader = new StreamReader(source, encoding);

            var header1 = await reader.ReadLineAsync();
            var header2 = await reader.ReadLineAsync();

            await using var file = new FileStream(destination, FileMode.Create, FileAccess.Write);
            await using var writer = new StreamWriter(file, encoding);

            await WriteHeaders(writer, header1, header2);
            while (!reader.EndOfStream)
            {
                var sequence = new List<Element>();
                for (var i = 0; i < _sequenceLength; i++)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }

                    var data = line.Split(',');

                    var current = Element.Parse(data[_layout.Year], data[_layout.Term],
                        data[_layout.Category], data[_layout.Subject], data[_layout.Score]);
                    sequence.Add(current);

                    var previousElements = sequence
                        .Where(e => e.Term < current.Term)
                        .ToArray();
                    var relevantElements = previousElements
                        .Where(e => e.Category == current.Category)
                        .ToArray();

                    var stats = (relevantElements.Length, previousElements.Length) switch
                    {
                        (0, _) => _analyzer.Analyze(previousElements.Select(e => e.Score)),
                        _ => _analyzer.Analyze(relevantElements.Select(e => e.Score))
                    };

                    await WriteLine(writer, data, stats);
                }
            }

            _layout.AddStats(_statsSize);
            return destination;
        }

        private async Task WriteLine(StreamWriter writer, string[] data, StatValues stats)
        {
            var result = new List<string>();
            result.AddRange(data[.._layout.Score]);
            result.AddRange(StatsHeaders.Select(h =>
                h.Expr(stats).ToString(CultureInfo.InvariantCulture)));
            result.Add(data[_layout.Score]);

            await writer.WriteLineAsync(string.Join(',', result));
        }

        private async Task WriteHeaders(StreamWriter writer, string header1, string header2)
        {
            var headersCount = header1.Split(',').Length + _statsSize;
            var newMarks = new List<string>();
            for (var i = 1; i < headersCount; i++)
            {
                newMarks.Add("X" + i);
            }

            newMarks.Add("D1");
            await writer.WriteLineAsync(string.Join(',', newMarks));

            var headersTitles = header2.Split(',');
            var newTitles = headersTitles[..^1].ToList();
            newTitles.AddRange(StatsHeaders.Select(h => h.Header));
            newTitles.Add(headersTitles.Last());
            await writer.WriteLineAsync(string.Join(',', newTitles));
        }

        private struct Element
        {
            public int Year;
            public int Term;
            public int Category;
            public int Subject;
            public double Score;

            public static Element Parse(string year, string term, string category, string subject, string score)
            {
                return new Element
                {
                    Year = int.Parse(year),
                    Term = int.Parse(term),
                    Category = int.Parse(category),
                    Subject = int.Parse(subject),
                    Score = double.Parse(score)
                };
            }
        }
    }
}
