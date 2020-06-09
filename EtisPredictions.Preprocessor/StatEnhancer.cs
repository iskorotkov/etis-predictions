using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace EtisPredictions.Preprocessor
{
    public class StatEnhancer
    {
        private readonly int _sequenceLength;
        private readonly StatAnalyzer _analyzer;

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

        public StatEnhancer(int sequenceLength = 30, double defaultValue = 80.0)
        {
            _sequenceLength = sequenceLength;
            _analyzer = new StatAnalyzer(defaultValue);
        }

        public async Task AddStatisticsParams(string input, string output)
        {
            using var reader = new StreamReader(input);

            var header1 = await reader.ReadLineAsync();
            var header2 = await reader.ReadLineAsync();

            await using var writer = new StreamWriter(output);
            await WriteHeaders(writer, header1, header2);
            while (!reader.EndOfStream)
            {
                var sequence = new List<Element>();
                for (var i = 0; i < _sequenceLength; i++)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        return;
                    }

                    var data = line.Split(',');

                    var current = Element.Parse(data[0], data[1],
                        data[2], data[3], data[12]);
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
        }

        private async Task WriteLine(StreamWriter writer, string[] data, StatValues stats)
        {
            var result = new List<string>();
            result.AddRange(data[..^1]);
            result.AddRange(new[]
            {
                stats.Min, stats.Max, stats.Median, stats.Variance, stats.Percentile25, stats.Percentile75,
                stats.Percentile10, stats.Percentile90
            }.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            result.Add(data.Last());

            await writer.WriteLineAsync(string.Join(',', result));
        }

        private async Task WriteHeaders(StreamWriter writer, string header1, string header2)
        {
            var headersCount = header1.Split(',').Length + 8;
            var newMarks = new List<string>();
            for (var i = 1; i < headersCount; i++)
            {
                newMarks.Add("X" + i);
            }

            newMarks.Add("D1");
            await writer.WriteLineAsync(string.Join(',', newMarks));

            var headersTitles = header2.Split(',');
            var newTitles = headersTitles[..^1].ToList();
            newTitles.AddRange(new[]
            {
                "Min", "Max", "Median", "Variance", "Percentile 25", "Percentile 75", "Percentile 10", "Percentile 90"
            });
            newTitles.Add(headersTitles.Last());
            await writer.WriteLineAsync(string.Join(',', headersTitles));
        }
    }
}
