using System;

namespace EtisPredictions.Preprocessor
{
    public class Layout
    {
        public Index Year { get; }
        public Index Term { get; }
        public Index Category { get; }
        public Index Subject { get; }
        public Index Grant { get; }
        public Index Score { get; }

        public Range NumericValues { get; private set; }
        public Range Stats { get; private set; }

        public void AddStats(int size)
        {
            Stats = new Range(new Index(Score.Value + size, true), Score);
        }

        public Layout()
        {
            Year = 0;
            Term = 1;
            Category = 2;
            Subject = 3;
            Grant = 9;
            Score = ^1;
            NumericValues = new Range(Subject.Value + 1, Score);
            Stats = new Range(Score, Score);
        }
    }
}
