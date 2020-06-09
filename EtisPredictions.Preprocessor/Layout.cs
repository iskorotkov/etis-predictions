using System;

namespace EtisPredictions.Preprocessor
{
    public class Layout
    {
        public Index Year;
        public Index Term;
        public Index Category;
        public Index Subject;
        public Index Grant;
        public Index Score;

        public Range UninterestingData;

        public Layout()
        {
            Year = 0;
            Term = 1;
            Category = 2;
            Subject = 3;
            Grant = 9;
            Score = ^1;
            UninterestingData = new Range(Subject.Value + 1, Score);
        }
    }
}
