using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EtisPredictions.Preprocessor
{
    static class Program
    {
        private const string BasePath = @"C:\Users\korot\OneDrive\Docs\2019-2020\ИС";

        private static string InputFile => Path.Combine(BasePath, "input.csv");
        private static string OutputFile => Path.Combine(BasePath, "output.csv");
        private static string BufferFile(int index) => Path.Combine(BasePath, $"buffer{index}.csv");

        static async Task Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encoding = Encoding.GetEncoding(1251);
            
            var layout = new Layout();
            await new StatEnhancer(layout).AddStatisticsParams(InputFile, BufferFile(1), encoding);
            await new DataAugmenter(layout).AddAugmentedData(BufferFile(1), BufferFile(2), encoding);
            await new DataShuffler().ShuffleData(BufferFile(2), encoding);
            await new OneHotEncoder(layout).UseOneHotEncoding(BufferFile(2), OutputFile, encoding);
        }
    }
}
