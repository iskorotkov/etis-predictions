using System;
using System.Linq;
using EtisPredictionsML.Model;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EtisPredictionsML.ConsoleApp
{
    class Program
    {
        // ReSharper disable once UnusedMember.Local
        private static void Main(string data, string model)
        {
            var context = new MLContext(1);
            var dataView = context.Data.LoadFromTextFile<ModelInput>(data, hasHeader: true, separatorChar: ',');

            var transformer = context.Model.Load(model, out _);
            var predictions = transformer.Transform(dataView);
            var metrics = context.Regression.Evaluate(predictions, "D1");
            Console.WriteLine($"RSquared: {metrics.RSquared}, root mean squared error: {metrics.RootMeanSquaredError}");

            var actualValues = predictions.GetColumn<float>("D1");
            var predictedValues = predictions.GetColumn<float>("Score");
            foreach (var (actual, predicted) in actualValues.Zip(predictedValues))
            {
                Console.WriteLine($"Actual: {actual}, predicted: {predicted}, error: {predicted - actual}");
            }
        }
    }
}
