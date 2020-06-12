using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor.Passes
{
    public class DataAugmenter : IPass
    {
        private readonly double _minLevel;
        private readonly double _maxLevel;
        private readonly Layout _layout;
        private readonly int _times;

        public DataAugmenter(Layout layout, double minLevel = 0.95, double maxLevel = 1.05, int times = 50)
        {
            _times = times;
            _layout = layout;
            _minLevel = minLevel;
            _maxLevel = maxLevel;
        }

        public async Task<string> Do(string source, string destination, Encoding encoding)
        {
            using var reader = new StreamReader(source, encoding);
            await using var file = new FileStream(destination, FileMode.Create, FileAccess.Write);
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

                await writer.WriteLineAsync(line);

                var data = line.Split(',');
                for (var i = 0; i < _times; i++)
                {
                    var augmented = new List<string>();
                    augmented.AddRange(data[.._layout.Grant]);
                    augmented.Add(Augment(random, data[_layout.Grant]).ToString(CultureInfo.InvariantCulture));
                    augmented.AddRange(data[(_layout.Grant.Value + 1).._layout.Stats.Start]);

                    augmented.AddRange(data[_layout.Stats]
                        .Select(value => Augment(random, value)
                            .ToString(CultureInfo.InvariantCulture)));

                    augmented.Add(Augment(random, data[_layout.Score])
                        .ToString(CultureInfo.InvariantCulture));

                    await writer.WriteLineAsync(string.Join(',', augmented));
                }
            }

            return destination;
        }

        private double Augment(Random random, string data)
        {
            var value = double.Parse(data);
            var multiplier = _minLevel + (_maxLevel - _minLevel) * random.NextDouble();
            return value * multiplier;
        }
    }
}
