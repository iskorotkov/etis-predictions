using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor.Passes
{
    public class DataShuffler
    {
        private readonly Random _random = new Random();
        
        private void Shuffle<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = _random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public async Task ShuffleData(string from, string to, Encoding encoding)
        {
            var data = new List<string>();
            string header1;
            string header2;
            using (var reader = new StreamReader(from, encoding))
            {
                header1 = await reader.ReadLineAsync();
                header2 = await reader.ReadLineAsync();
                while (!reader.EndOfStream)
                {
                    data.Add(await reader.ReadLineAsync());
                }
            }

            Shuffle(data);

            await using var fileStream = new FileStream(to, FileMode.Create, FileAccess.Write);
            await using var writer = new StreamWriter(fileStream, encoding);
            await writer.WriteLineAsync(header1);
            await writer.WriteLineAsync(header2);
            foreach (var line in data)
            {
                await writer.WriteLineAsync(line);
            }
        }
    }
}
