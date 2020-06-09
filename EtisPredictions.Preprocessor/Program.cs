using System;
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
            await new StatEnhancer().AddStatisticsParams(InputFile, BufferFile(1));
            await new DataAugmenter().AddAugmentedData(BufferFile(1), BufferFile(2));
            await new DataShuffler().ShuffleData(BufferFile(2));
            await new OneHotEncoder().UseOneHotEncoding(BufferFile(2), OutputFile);
        }

        public struct Layout
        {
            public Index Year;
            public Index Term;
            public Index Category;
            public Index Subject;
        }
    }
}
