namespace EtisPredictions.Preprocessor.Statistics
{
    public struct StatValues
    {
        public double Min;
        public double Max;
        public double Mean;
        public double Median;
        public double Variance;
        public double Percentile25;
        public double Percentile75;
        public double Percentile10;
        public double Percentile90;

        public static StatValues GetDefault(double value)
        {
            return new StatValues
            {
                Min = value,
                Max = value,
                Mean = value,
                Median = value,
                Variance = value,
                Percentile10 = value,
                Percentile25 = value,
                Percentile75 = value,
                Percentile90 = value
            };
        }
    }
}
