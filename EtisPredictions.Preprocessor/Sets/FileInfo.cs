using System;

namespace EtisPredictions.Preprocessor.Sets
{
    public class FileInfo
    {
        public string Filename { get; }
        public string Set { get; }
        public bool IsTrain { get; }

        public FileInfo(string filename, string set, bool isTrain)
        {
            Filename = filename ?? throw new ArgumentNullException(nameof(filename));
            Set = set ?? throw new ArgumentNullException(nameof(set));
            IsTrain = isTrain;
        }
    }
}
