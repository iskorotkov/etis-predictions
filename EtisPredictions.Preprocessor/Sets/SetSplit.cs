using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor.Sets
{
    public class SetSplit
    {
        public async Task Split(string from, Files files, Rates rates, Encoding encoding)
        {
            using var reader = new StreamReader(from, encoding);
            var header1 = await reader.ReadLineAsync();
            var header2 = await reader.ReadLineAsync();

            await using var trainFile = new FileStream(files.Train, FileMode.Create);
            await using var valFile = new FileStream(files.Val, FileMode.Create);
            await using var testFile = new FileStream(files.Test, FileMode.Create);

            await using var trainWriter = new StreamWriter(trainFile, encoding);
            await using var valWriter = new StreamWriter(valFile, encoding);
            await using var testWriter = new StreamWriter(testFile, encoding);

            foreach (var writer in new[] { trainWriter, valWriter, testWriter })
            {
                await writer.WriteLineAsync(header1);
                await writer.WriteLineAsync(header2);
            }

            var random = new Random();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var output = random.NextDouble() switch
                {
                    { } x when x < rates.Test => testWriter,
                    { } x when x < rates.Test + rates.Val => valWriter,
                    _ => trainWriter
                };
                await output.WriteLineAsync(line);
            }
        }
    }
}
