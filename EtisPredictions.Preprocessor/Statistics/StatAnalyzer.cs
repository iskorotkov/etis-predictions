using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace EtisPredictions.Preprocessor.Statistics
{
    public class StatAnalyzer
    {
        private readonly double _defaultValue;

        public StatAnalyzer(double defaultValue)
        {
            _defaultValue = defaultValue;
        }

        public StatValues Analyze(IEnumerable<double> values)
        {
            var arr = values.ToArray();
            return arr.Length switch
            {
                0 => StatValues.GetDefault(_defaultValue),
                1 => StatValues.GetDefault(arr[0]),
                _ => new StatValues
                {
                    Min = arr.Min(),
                    Max = arr.Max(),
                    Mean = arr.Average(),
                    Median = ArrayStatistics.MedianInplace(arr),
                    Variance = ArrayStatistics.Variance(arr),
                    Percentile25 = ArrayStatistics.LowerQuartileInplace(arr),
                    Percentile75 = ArrayStatistics.UpperQuartileInplace(arr),
                    Percentile10 = ArrayStatistics.PercentileInplace(arr, 10),
                    Percentile90 = ArrayStatistics.PercentileInplace(arr, 90)
                }
            };
        }
    }
}
