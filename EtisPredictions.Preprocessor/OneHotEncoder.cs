using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor
{
    public class OneHotEncoder
    {
        private readonly Layout _layout;
        private readonly bool _oneHotYears;
        private readonly bool _oneHotTerms;
        private readonly bool _oneHotCategories;
        private readonly bool _oneHotSubjects;
        private const int Years = 4;
        private const int Terms = 12;
        private const int Categories = 10;
        private const int Subjects = 50;

        public OneHotEncoder(Layout layout,
            bool oneHotYears = true,
            bool oneHotTerms = true,
            bool oneHotCategories = true,
            bool oneHotSubjects = true)
        {
            _layout = layout;
            _oneHotYears = oneHotYears;
            _oneHotTerms = oneHotTerms;
            _oneHotCategories = oneHotCategories;
            _oneHotSubjects = oneHotSubjects;
        }

        public async Task UseOneHotEncoding(string from, string to)
        {
            using var reader = new StreamReader(@from);
            await using var writer = new StreamWriter(to);

            await WriteFirstHeaders(writer, await reader.ReadLineAsync());
            await WriteSecondHeaders(writer, await reader.ReadLineAsync());

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                var data = line.Split(',');

                var processed = new List<string>();
                if (_oneHotYears)
                {
                    processed.AddRange(ToOneHot(data[_layout.Year], Years));
                }
                else
                {
                    processed.Add(data[_layout.Year]);
                }

                if (_oneHotTerms)
                {
                    processed.AddRange(ToOneHot(data[_layout.Term], Terms));
                }
                else
                {
                    processed.Add(data[_layout.Term]);
                }

                if (_oneHotCategories)
                {
                    processed.AddRange(ToOneHot(data[_layout.Category], Categories));
                }
                else
                {
                    processed.Add(data[_layout.Category]);
                }

                if (_oneHotSubjects)
                {
                    processed.AddRange(ToOneHot(data[_layout.Subject], Subjects));
                }
                else
                {
                    processed.Add(data[_layout.Subject]);
                }

                processed.AddRange(data[_layout.UninterestingData]);
                processed.Add(data[_layout.Score]);
                await writer.WriteLineAsync(string.Join(',', processed));
            }
        }

        private IEnumerable<string> ToOneHot(string selected, int total)
        {
            var value = int.Parse(selected);
            var encoded = Enumerable.Repeat("0", total).ToList();
            encoded[value - 1] = "1";
            return encoded;
        }

        private async Task WriteSecondHeaders(StreamWriter writer, string originalHeaders)
        {
            var titles = originalHeaders.Split(',').ToArray();
            var secondHeaders = new List<string>();
            if (_oneHotYears)
            {
                for (var i = 1; i <= Years; i++)
                {
                    secondHeaders.Add("Курс " + i);
                }
            }
            else
            {
                secondHeaders.Add(titles[_layout.Year]);
            }

            if (_oneHotTerms)
            {
                for (var i = 1; i <= Terms; i++)
                {
                    secondHeaders.Add("Триместр " + i);
                }
            }
            else
            {
                secondHeaders.Add(titles[_layout.Term]);
            }

            if (_oneHotCategories)
            {
                for (var i = 1; i <= Categories; i++)
                {
                    secondHeaders.Add("Категория " + i);
                }
            }
            else
            {
                secondHeaders.Add(titles[_layout.Category]);
            }

            if (_oneHotSubjects)
            {
                for (var i = 1; i <= Subjects; i++)
                {
                    secondHeaders.Add("Предмет " + i);
                }
            }
            else
            {
                secondHeaders.Add(titles[_layout.Subject]);
            }

            secondHeaders.AddRange(titles[_layout.UninterestingData]);
            secondHeaders.Add(titles[_layout.Score]);
            await writer.WriteLineAsync(string.Join(',', secondHeaders));
        }

        private async Task WriteFirstHeaders(StreamWriter writer, string originalHeaders)
        {
            var originalCount = originalHeaders.Count(c => c == ',');
            if (_oneHotYears)
            {
                originalCount += Years - 1;
            }

            if (_oneHotTerms)
            {
                originalCount += Terms - 1;
            }

            if (_oneHotCategories)
            {
                originalCount += Categories - 1;
            }

            if (_oneHotSubjects)
            {
                originalCount += Subjects - 1;
            }

            var firstHeaders = new List<string>();
            for (var i = 1; i <= originalCount; i++)
            {
                firstHeaders.Add("X" + i);
            }

            firstHeaders.Add("D1");
            await writer.WriteLineAsync(string.Join(',', firstHeaders));
        }
    }
}
