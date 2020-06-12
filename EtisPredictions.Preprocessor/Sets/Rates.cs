using System;

namespace EtisPredictions.Preprocessor.Sets
{
    public class Rates
    {
        public double Train => 1.0 - Val - Test;
        public double Val { get; }
        public double Test { get; }

        public Rates(double val, double test)
        {
            if (val < 0.0 || test < 0.0)
            {
                throw new ArgumentException("Split rates can't ne negative.");
            }

            if (val + test > 1.0)
            {
                throw new ArgumentException("Sum of split rates is more then 1.");
            }

            Val = val;
            Test = test;
        }
    }
}
