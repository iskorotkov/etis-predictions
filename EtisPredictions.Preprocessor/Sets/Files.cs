using System;
using System.Collections.Generic;

namespace EtisPredictions.Preprocessor.Sets
{
    public class Files
    {
        public string Train { get; }
        public string Val { get; }
        public string Test { get; }

        public Files(string train, string val, string test)
        {
            Train = train ?? throw new ArgumentNullException(nameof(train));
            Val = val ?? throw new ArgumentNullException(nameof(val));
            Test = test ?? throw new ArgumentNullException(nameof(test));
        }

        public IEnumerable<FileInfo> GetInfo()
        {
            return new[]
            {
                new FileInfo(Train, "train", true),
                new FileInfo(Val, "val", false),
                new FileInfo(Test, "test", false)
            };
        }
    }
}
