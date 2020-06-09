using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor
{
    public class DataAugmenter
    {
        private readonly double _minLevel;
        private readonly double _maxLevel;

        public DataAugmenter(double minLevel = 0.95, double maxLevel = 1.05)
        {
            _minLevel = minLevel;
            _maxLevel = maxLevel;
        }

        public async Task AddAugmentedData(string from, string to, int times = 50)
        {
            using var reader = new StreamReader(@from);
            await using var writer = new StreamWriter(to);

            await writer.WriteLineAsync(await reader.ReadLineAsync());
            await writer.WriteLineAsync(await reader.ReadLineAsync());

            var random = new Random();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                var data = line.Split(',');

                await writer.WriteLineAsync(line);

                for (var i = 0; i < times; i++)
                {
                    var augmented = new List<string>();
                    augmented.AddRange(data[..9]);
                    augmented.Add(Augment(random, data[9]).ToString(CultureInfo.InvariantCulture));
                    augmented.Add(data[10]);
                    augmented.Add(Math.Clamp(Augment(random, data[11]), 0, 100).ToString(CultureInfo.InvariantCulture));
                    augmented.Add(Math.Clamp(Augment(random, data[12]), 0, 100).ToString(CultureInfo.InvariantCulture));

                    await writer.WriteLineAsync(string.Join(',', augmented));
                }
            }
        }

        private double Augment(Random random, string data)
        {
            var value = double.Parse(data);
            var multiplier = _minLevel + (_maxLevel - _minLevel) * random.NextDouble();
            return value * multiplier;
        }
    }
}
