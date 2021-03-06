﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor.Passes
{
    public class OneHotEncoder : IPass
    {
        private readonly Layout _layout;
        private readonly bool _oneHotYears;
        private readonly bool _oneHotTerms;
        private readonly bool _oneHotCategories;
        private readonly bool _oneHotSubjects;
        private OheConfig _config;

        public OneHotEncoder(
            Layout layout,
            OheConfig config
        )
        {
            _config = config;
            _layout = layout;
            _oneHotYears = _config.Years > 1;
            _oneHotTerms = _config.Terms > 1;
            _oneHotCategories = _config.Categories > 1;
            _oneHotSubjects = _config.Subjects > 1;
        }

        public class OheConfig
        {
            public OheConfig(int years, int terms, int categories, int subjects)
            {
                if (years <= 0 || terms <= 0 || categories <= 0 || subjects <= 0)
                {
                    throw new ArgumentException("Can't encode categories with non-positive number of values.");
                }

                Years = years;
                Terms = terms;
                Categories = categories;
                Subjects = subjects;
            }

            public int Years { get; }
            public int Terms { get; }
            public int Categories { get; }
            public int Subjects { get; }
        }

        public async Task<string> Do(string source, string destination, Encoding encoding)
        {
            using var reader = new StreamReader(source, encoding);
            await using var file = new FileStream(destination, FileMode.Create, FileAccess.Write);
            await using var writer = new StreamWriter(file, encoding);

            await WriteFirstHeaders(writer, await reader.ReadLineAsync());
            await WriteSecondHeaders(writer, await reader.ReadLineAsync());

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                var data = line.Split(',');

                var processed = new List<string>();
                if (_oneHotYears)
                {
                    processed.AddRange(ToOneHot(data[_layout.Year], _config.Years));
                }
                else
                {
                    processed.Add(data[_layout.Year]);
                }

                if (_oneHotTerms)
                {
                    processed.AddRange(ToOneHot(data[_layout.Term], _config.Terms));
                }
                else
                {
                    processed.Add(data[_layout.Term]);
                }

                if (_oneHotCategories)
                {
                    processed.AddRange(ToOneHot(data[_layout.Category], _config.Categories));
                }
                else
                {
                    processed.Add(data[_layout.Category]);
                }

                if (_oneHotSubjects)
                {
                    processed.AddRange(ToOneHot(data[_layout.Subject], _config.Subjects));
                }
                else
                {
                    processed.Add(data[_layout.Subject]);
                }

                processed.AddRange(data[_layout.NumericValues]);
                processed.Add(data[_layout.Score]);
                await writer.WriteLineAsync(string.Join(',', processed));
            }

            return destination;
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
                for (var i = 1; i <= _config.Years; i++)
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
                for (var i = 1; i <= _config.Terms; i++)
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
                for (var i = 1; i <= _config.Categories; i++)
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
                for (var i = 1; i <= _config.Subjects; i++)
                {
                    secondHeaders.Add("Предмет " + i);
                }
            }
            else
            {
                secondHeaders.Add(titles[_layout.Subject]);
            }

            secondHeaders.AddRange(titles[_layout.NumericValues]);
            secondHeaders.Add(titles[_layout.Score]);
            await writer.WriteLineAsync(string.Join(',', secondHeaders));
        }

        private async Task WriteFirstHeaders(StreamWriter writer, string originalHeaders)
        {
            var originalCount = originalHeaders.Count(c => c == ',');
            if (_oneHotYears)
            {
                originalCount += _config.Years - 1;
            }

            if (_oneHotTerms)
            {
                originalCount += _config.Terms - 1;
            }

            if (_oneHotCategories)
            {
                originalCount += _config.Categories - 1;
            }

            if (_oneHotSubjects)
            {
                originalCount += _config.Subjects - 1;
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
