using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor
{
    public class OneHotEncoder
    {
        private readonly bool _oneHotYears;
        private readonly bool _oneHotTerms;
        private readonly bool _oneHotCategories;
        private readonly bool _oneHotSubjects;
        private const int Years = 4;
        private const int Terms = 12;
        private const int Categories = 10;
        private const int Subjects = 50;
        private const int OtherParams = 8;

        public OneHotEncoder(
            bool oneHotYears = true,
            bool oneHotTerms = true,
            bool oneHotCategories = true,
            bool oneHotSubjects = true
        )
        {
            _oneHotYears = oneHotYears;
            _oneHotTerms = oneHotTerms;
            _oneHotCategories = oneHotCategories;
            _oneHotSubjects = oneHotSubjects;
        }

        private int CountFeatures()
        {
            return (_oneHotYears ? Years : 1) +
                   (_oneHotTerms ? Terms : 1) +
                   (_oneHotCategories ? Categories : 1) +
                   (_oneHotSubjects ? Subjects : 1) + OtherParams;
        }

        public async Task UseOneHotEncoding(string from, string to)
        {
            using var reader = new StreamReader(@from);
            await using var writer = new StreamWriter(to);
            var features = CountFeatures();

            await SkipHeadersRows(reader);
            await WriteFirstHeaders(writer, features);
            await WriteSecondHeaders(writer);

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
                    processed.AddRange(ToOneHot(data[0], Years));
                }
                else
                {
                    processed.Add(data[0]);
                }

                if (_oneHotTerms)
                {
                    processed.AddRange(ToOneHot(data[1], Terms));
                }
                else
                {
                    processed.Add(data[1]);
                }

                if (_oneHotCategories)
                {
                    processed.AddRange(ToOneHot(data[2], Categories));
                }
                else
                {
                    processed.Add(data[2]);
                }

                if (_oneHotSubjects)
                {
                    processed.AddRange(ToOneHot(data[3], Subjects));
                }
                else
                {
                    processed.Add(data[3]);
                }

                processed.AddRange(data[4..]);
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

        private async Task SkipHeadersRows(StreamReader reader)
        {
            _ = await reader.ReadLineAsync();
            _ = await reader.ReadLineAsync();
        }

        private async Task WriteSecondHeaders(StreamWriter writer)
        {
            var secondHeaders = new List<string>();
            for (var i = 1; i <= Years; i++)
            {
                secondHeaders.Add("Курс " + i);
            }

            for (var i = 1; i <= Terms; i++)
            {
                secondHeaders.Add("Триместр " + i);
            }

            for (var i = 1; i <= Categories; i++)
            {
                secondHeaders.Add("Категория " + i);
            }

            for (var i = 1; i <= Subjects; i++)
            {
                secondHeaders.Add("Предмет" + i);
            }

            secondHeaders.Add(
                "Экзамен (или зачет),Часов аудиторной работы,КТ,Профильный?,Пропусков,Размер стипендии,Наличие интереса,Средний балл по категории в прошлом,Оценка");
            await writer.WriteLineAsync(string.Join(',', secondHeaders));
        }

        private async Task WriteFirstHeaders(StreamWriter writer, int features)
        {
            var firstHeaders = new List<string>();
            for (var i = 1; i <= features; i++)
            {
                firstHeaders.Add("X" + i);
            }

            firstHeaders.Add("D1");
            await writer.WriteLineAsync(string.Join(',', firstHeaders));
        }
    }
}
