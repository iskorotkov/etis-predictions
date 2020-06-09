using System.IO;
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
            await new DataAugmenter().AddAugmentedData(InputFile, BufferFile(1));
            await new DataShuffler().ShuffleData(BufferFile(1));
            await new OneHotEncoder().UseOneHotEncoding(BufferFile(1), OutputFile);
        }
    }
}
