using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor.Sets
{
    public class SetSplit
    {
        private string _header1;
        private string _header2;

        public async Task Split(string from, Files files, Rates rates, Encoding encoding)
        {
            var lines = await ReadFile(from, encoding);
            var valLines = (int) Math.Round(lines.Count * rates.Val);
            var testLines = (int) Math.Round(lines.Count * rates.Test);
            var linesArr = lines.ToArray();

            await WriteSet(files.Val, linesArr[..valLines], encoding);
            await WriteSet(files.Test, linesArr[valLines..(valLines + testLines)], encoding);
            await WriteSet(files.Train, linesArr[(valLines + testLines)..], encoding);
        }

        private async Task<List<string>> ReadFile(string from, Encoding encoding)
        {
            using var reader = new StreamReader(from, encoding);
            _header1 = await reader.ReadLineAsync();
            _header2 = await reader.ReadLineAsync();
            return await ReadAllLines(reader);
        }

        private static async Task<List<string>> ReadAllLines(StreamReader reader)
        {
            var lines = new List<string>();
            while (!reader.EndOfStream)
            {
                lines.Add(await reader.ReadLineAsync());
            }

            return lines;
        }

        private async Task WriteSet(string filename, IEnumerable<string> lines, Encoding encoding)
        {
            await using var file = new FileStream(filename, FileMode.Create);
            await using var writer = new StreamWriter(file, encoding);
            await writer.WriteLineAsync(_header1);
            await writer.WriteLineAsync(_header2);
            foreach (var line in lines)
            {
                await writer.WriteLineAsync(line);
            }
        }
    }
}
