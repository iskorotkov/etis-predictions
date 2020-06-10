using System.IO;
using System.Text;
using System.Threading.Tasks;
using EtisPredictions.Preprocessor.Passes;

namespace EtisPredictions.Preprocessor
{
    static class Program
    {
        private static int _buffersUsed;
        private static string _buffersFolder;

        private static string NextBufferFile()
        {
            var result = Path.Combine(_buffersFolder, $"{_buffersUsed}.csv");
            _buffersUsed++;
            return result;
        }

        private static async Task Main(
            string trainFile = @"train.csv",
            string validationFile = @"val.csv",
            string testFile = @"test.csv",
            string inputFolder = "input",
            string outputFolder = "output",
            string buffers = @"buffers",
            bool addStats = true,
            bool augment = true,
            bool shuffle = true,
            bool useOhe = true
        )
        {
            _buffersFolder = buffers;
            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encoding = Encoding.GetEncoding(1251);
            var layout = new Layout();

            foreach (var filename in new[] { trainFile, validationFile, testFile })
            {
                var from = Path.Combine(inputFolder, filename);
                var to = NextBufferFile();
                if (addStats)
                {
                    await new StatEnhancer(layout).AddStatisticsParams(from, to, encoding);
                    from = to;
                    to = NextBufferFile();
                }

                if (augment && filename == trainFile)
                {
                    await new DataAugmenter(layout).AddAugmentedData(from, to, encoding);
                    from = to;
                    to = NextBufferFile();

                }

                if (shuffle)
                {
                    await new DataShuffler().ShuffleData(from, encoding);

                }

                if (useOhe)
                {
                    await new OneHotEncoder(layout).UseOneHotEncoding(from, to, encoding);
                    from = to;
                }

                File.Move(from, Path.Combine(outputFolder, filename), true);
                _buffersUsed--;
            }
        }
    }
}
