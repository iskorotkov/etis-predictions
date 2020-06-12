using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EtisPredictions.Preprocessor.Passes;
using EtisPredictions.Preprocessor.Sets;
using XlsxCsvConversions.Converters;

namespace EtisPredictions.Preprocessor
{
    static class Program
    {
        private static int _buffersUsed;
        private static string _buffersFolder;
        private static bool _addStats;
        private static bool _augment;
        private static bool _shuffle;
        private static bool _useOhe;
        private static bool _useXlsx;
        private static Layout _layout = new Layout();
        private static Encoding _encoding;
        private static string _output;
        private static double _valRate;
        private static double _testRate;
        private static string _inputFile;
        private static bool _clearBuffers;
        private static string _sheetName;

        private static string NextBufferFile(string extension = "csv")
        {
            var result = Path.Combine(_buffersFolder, $"{_buffersUsed}.{extension}");
            _buffersUsed++;
            return result;
        }

        private static async Task Main(
            string input = "input/dataset.xlsx",
            string sheet = "dataset",
            string output = "output",
            string buffers = "buffers",
            double valRate = 0.15,
            double testRate = 0.15,
            bool stats = true,
            bool augment = true,
            bool shuffle = true,
            bool ohe = true,
            bool xlsx = true,
            bool cleanup = true
        )
        {
            _sheetName = sheet;
            _clearBuffers = cleanup;
            _inputFile = input;
            _testRate = testRate;
            _valRate = valRate;
            _output = output;
            _useXlsx = xlsx;
            _useOhe = ohe;
            _shuffle = shuffle;
            _augment = augment;
            _addStats = stats;
            _buffersFolder = buffers;
            _encoding = GetCorrectEncoding();

            if (_useXlsx)
            {
                _inputFile = await MakeCsvFile();
            }

            var resultFiles = await MakePasses(_inputFile);
            if (_useXlsx)
            {
                var xlsxResult = await MergeIntoXlsx(resultFiles);
                MoveToOutputFolder(xlsxResult);
            }
            else
            {
                MoveToOutputFolder(resultFiles);
            }

            Cleanup();
        }

        private static void MoveToOutputFolder(Files files)
        {
            foreach (var info in files.GetInfo())
            {
                File.Move(info.Filename, Path.Join(_output, info.Set));
            }
        }

        private static void Cleanup()
        {
            if (_clearBuffers)
            {
                foreach (var file in Directory.EnumerateFiles(_buffersFolder))
                {
                    File.Delete(file);
                }
            }
        }

        private static string PrepareXlsxBuffer()
        {
            var buffer = NextBufferFile("xlsx");
            if (File.Exists(buffer))
            {
                File.Delete(buffer);
            }

            return buffer;
        }

        private static Encoding GetCorrectEncoding()
        {
            if (_useXlsx)
            {
                return Encoding.Default;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(1251);
        }

        private static async Task<string> MakeCsvFile()
        {
            var converter = new XlsxToCsvConverter();
            var csvFile = NextBufferFile();
            await converter.Convert(_inputFile, _sheetName, csvFile);
            return csvFile;
        }

        private static async Task<string> MergeIntoXlsx(Files files)
        {
            var buffer = PrepareXlsxBuffer();
            var converter = new CsvToXlsxConverter();
            foreach (var info in files.GetInfo())
            {
                await converter.Convert(info.Filename, buffer, info.Set);
            }

            return buffer;
        }

        private static void MoveToOutputFolder(string from)
        {
            File.Move(from, _output, true);
            _buffersUsed--;
        }

        private static async Task<Files> MakePasses(string from)
        {
            if (_addStats)
            {
                var to = NextBufferFile();
                await new StatEnhancer(_layout).AddStatisticsParams(from, to, _encoding);
                from = to;
            }

            var trainFile = NextBufferFile();
            var valFile = NextBufferFile();
            var testFile = NextBufferFile();
            var files = new Files(trainFile, valFile, testFile);
            await new SetSplit().Split(from, files, new Rates(_valRate, _testRate), _encoding);

            var resultFiles = new List<string>();
            foreach (var info in files.GetInfo())
            {
                resultFiles.Add(await MakePassesOnSet(info.Filename, info.IsTrain));
            }

            return new Files(resultFiles[0], resultFiles[1], resultFiles[2]);
        }

        private static async Task<string> MakePassesOnSet(string from, bool train)
        {
            if (_augment && train)
            {
                var to = NextBufferFile();
                await new DataAugmenter(_layout).AddAugmentedData(from, to, _encoding);
                from = to;
            }

            if (_shuffle)
            {
                var to = NextBufferFile();
                await new DataShuffler().ShuffleData(from, to, _encoding);
                from = to;
            }

            if (_useOhe)
            {
                var to = NextBufferFile();
                await new OneHotEncoder(_layout).UseOneHotEncoding(from, to, _encoding);
                from = to;
            }

            return from;
        }
    }
}
