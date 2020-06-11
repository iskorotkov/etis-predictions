using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor.Passes
{
    public class DataAugmenter
    {
        private readonly double _minLevel;
        private readonly double _maxLevel;
        private readonly Layout _layout;

        public DataAugmenter(Layout layout, double minLevel = 0.95, double maxLevel = 1.05)
        {
            _layout = layout;
            _minLevel = minLevel;
            _maxLevel = maxLevel;
        }

        public async Task AddAugmentedData(string from, string to, Encoding encoding, int times = 50)
        {
            using var reader = new StreamReader(@from, encoding);
            await using var file = new FileStream(to, FileMode.Create, FileAccess.Write);
            await using var writer = new StreamWriter(file, encoding);

            await writer.WriteLineAsync(await reader.ReadLineAsync());
            await writer.WriteLineAsync(await reader.ReadLineAsync());

            var random = new Random();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                var data = line.Split(',');

                await writer.WriteLineAsync(line);

                for (var i = 0; i < times; i++)
                {
                    var augmented = new List<string>();
                    augmented.AddRange(data[.._layout.Grant]);
                    augmented.Add(Augment(random, data[_layout.Grant]).ToString(CultureInfo.InvariantCulture));
                    augmented.AddRange(data[(_layout.Grant.Value + 1).._layout.Score]);
                    augmented.Add(Math.Clamp(Augment(random, data[_layout.Score]), 0, 100).ToString(CultureInfo.InvariantCulture));

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
