using System.IO;
using System.Text;
using System.Threading.Tasks;
using EtisPredictions.Preprocessor.Passes;
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
        private static string _csvTrainFile;
        private static string _csvValFile;
        private static string _csvTestFile;
        private static string _xlsxFile;
        private static string _trainSheet;
        private static string _valSheet;
        private static string _testSheet;
        private static bool _useXlsx;
        private static Layout _layout = new Layout();
        private static Encoding _encoding;
        private static string _inputFolder;
        private static string _outputFolder;
        private static string _buffers;

        private static string NextBufferFile()
        {
            var result = Path.Combine(_buffersFolder, $"{_buffersUsed}.csv");
            _buffersUsed++;
            return result;
        }

        private static async Task Main(
            string csvTrainFile = "train.csv",
            string csvValFile = "val.csv",
            string csvTestFile = "test.csv",
            string xlsxFile = "dataset.xlsx",
            string trainSheet = "train",
            string valSheet = "val",
            string testSheet = "test",
            string inputFolder = "input",
            string outputFolder = "output",
            string buffers = "buffers",
            bool addStats = true,
            bool augment = true,
            bool shuffle = true,
            bool useOhe = true,
            bool useXlsx = false
        )
        {
            _buffers = buffers;
            _outputFolder = outputFolder;
            _inputFolder = inputFolder;
            _useXlsx = useXlsx;
            _testSheet = testSheet;
            _valSheet = valSheet;
            _trainSheet = trainSheet;
            _xlsxFile = xlsxFile;
            _csvTestFile = csvTestFile;
            _csvValFile = csvValFile;
            _csvTrainFile = csvTrainFile;
            _useOhe = useOhe;
            _shuffle = shuffle;
            _augment = augment;
            _addStats = addStats;
            _buffersFolder = buffers;
            _encoding = GetCorrectEncoding();
            if (_useXlsx)
            {
                foreach (var sheet in new[] { _trainSheet, _valSheet, _testSheet })
                {
                    var @from = await PrepareXlsxFile(sheet);
                    var buffer = await MakePasses(from, sheet == _trainSheet);
                    await FinalizeXlsxFile(buffer, sheet);
                }
            }
            else
            {
                foreach (var filename in new[] { csvTrainFile, csvValFile, csvTestFile })
                {
                    var @from = await PrepareCsvFile(filename);
                    var buffer = await MakePasses(from, filename == _csvTrainFile);
                    await FinalizeCsvFiles(buffer, filename);
                }
            }
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

        private static async Task<string> PrepareXlsxFile(string sheet)
        {
            var converter = new XlsxToCsvConverter();
            var csvFile = NextBufferFile();
            await converter.Convert(Path.Combine(_inputFolder, _xlsxFile), sheet, csvFile);
            return csvFile;
        }

        private static Task<string> PrepareCsvFile(string sourceFile)
        {
            return Task.FromResult(Path.Combine(_inputFolder, sourceFile));
        }

        private static async Task FinalizeXlsxFile(string lastBuffer, string resultSheet)
        {
            var converter = new CsvToXlsxConverter();
            await converter.Convert(lastBuffer, Path.Combine(_outputFolder, _xlsxFile), resultSheet);
        }

        private static Task FinalizeCsvFiles(string lastBuffer, string resultFile)
        {
            File.Move(lastBuffer, Path.Combine(_outputFolder, resultFile), true);
            _buffersUsed--;
            return Task.CompletedTask;
        }

        private static async Task<string> MakePasses(string from, bool trainSet)
        {
            if (_addStats)
            {
                var to = NextBufferFile();
                await new StatEnhancer(_layout).AddStatisticsParams(@from, to, _encoding);
                @from = to;
            }

            if (_augment && trainSet)
            {
                var to = NextBufferFile();
                await new DataAugmenter(_layout).AddAugmentedData(@from, to, _encoding);
                @from = to;
            }

            if (_shuffle)
            {
                var to = NextBufferFile();
                await new DataShuffler().ShuffleData(@from, to, _encoding);
                from = to;
            }

            if (_useOhe)
            {
                var to = NextBufferFile();
                await new OneHotEncoder(_layout).UseOneHotEncoding(@from, to, _encoding);
                @from = to;
            }

            return from;
        }
    }
}
