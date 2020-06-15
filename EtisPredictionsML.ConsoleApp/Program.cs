using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.DragonFruit;
using System.Linq;
using System.Threading.Tasks;
using EtisPredictionsML.Model;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EtisPredictionsML.ConsoleApp
{
    public class Program
    {
        // ReSharper disable once UnusedMember.Local
        private static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();

            var loadCommand = new Command("load");
            loadCommand.ConfigureFromMethod(typeof(Program).GetMethod(nameof(EvaluateExistingModel)));
            rootCommand.AddCommand(loadCommand);

            var trainCommand = new Command("train");
            trainCommand.ConfigureFromMethod(typeof(Program).GetMethod(nameof(TrainAndEvaluateModel)));
            rootCommand.AddCommand(trainCommand);

            return await rootCommand.InvokeAsync(args);
        }

        public static void TrainAndEvaluateModel(string data, string model)
        {
            var context = new MLContext(11);
            var dataView = context.Data.LoadFromTextFile<ModelInput>(data, ',', true);
            var pipeline = ModelBuilder.BuildTrainingPipeline(context);
            var transformer = ModelBuilder.TrainModel(dataView, pipeline);
            Evaluate(context, dataView, transformer);
        }

        public static void EvaluateExistingModel(string data, string model)
        {
            var context = new MLContext(1);
            var dataView = context.Data.LoadFromTextFile<ModelInput>(data, hasHeader: true, separatorChar: ',');
            var transformer = context.Model.Load(model, out _);
            Evaluate(context, dataView, transformer);
        }

        private static void Evaluate(MLContext context, IDataView dataView, ITransformer transformer)
        {
            var predictions = transformer.Transform(dataView);
            PrintMetrics(context, predictions);

            var results = PrepareDataForEvaluation(predictions).ToArray();
            PrintResults(results);
            PrintMaxErrors(results);
            PrintErrorMedian(results);
            PrintMeanRelativeError(results);
        }

        private static void PrintMeanRelativeError(Record[] results)
        {
            var meanRelativeError = results.Select(x => 100.0 * x.Error / x.Actual).Average();
            Console.WriteLine($"Mean relative error: {meanRelativeError}%");
        }

        private static void PrintErrorMedian(Record[] results)
        {
            var errorMedian = MathNet.Numerics.Statistics.ArrayStatistics.MedianInplace(
                results.Select(x => x.Error).ToArray());
            Console.WriteLine($"Error median: {errorMedian}");
        }

        private static void PrintMaxErrors(Record[] results)
        {
            var maxErrors = results
                .Select(x => x.Error)
                .OrderByDescending(x => x)
                .Take(3);
            Console.WriteLine("Max errors:");
            foreach (var (error, index) in maxErrors.Zip(Enumerable.Range(1, 3)))
            {
                Console.WriteLine($"#{index}: {error}");
            }
        }

        private static void PrintResults(IEnumerable<Record> results)
        {
            foreach (var item in results)
            {
                Console.WriteLine(
                    $"#{item.Index} actual: {item.Actual}, predicted: {item.Predicted}, error: {item.Error}");
            }
        }

        private static void PrintMetrics(MLContext context, IDataView predictions)
        {
            var metrics = context.Regression.Evaluate(predictions, "D1");
            Console.WriteLine($"{nameof(metrics.LossFunction)}: {metrics.LossFunction}");
            Console.WriteLine($"{nameof(metrics.RSquared)}: {metrics.RSquared}");
            Console.WriteLine($"{nameof(metrics.MeanAbsoluteError)}: {metrics.MeanAbsoluteError} points");
            Console.WriteLine($"{nameof(metrics.MeanSquaredError)}: {metrics.MeanSquaredError}%");
            Console.WriteLine($"{nameof(metrics.RootMeanSquaredError)}: {metrics.RootMeanSquaredError}%");
        }

        private static IEnumerable<Record> PrepareDataForEvaluation(IDataView predictions)
        {
            var actualValues = predictions.GetColumn<float>("D1").ToArray();
            var predictedValues = predictions.GetColumn<float>("Score");
            var indices = Enumerable.Range(1, actualValues.Length);
            return indices
                .Zip(actualValues)
                .Zip(predictedValues)
                .Select(x => new Record
                {
                    Index = x.First.First,
                    Actual = x.First.Second,
                    Predicted = x.Second,
                    Error = Math.Abs(x.First.Second - x.Second)
                }).ToArray();
        }
    }

    public struct Record
    {
        public int Index;
        public float Actual;
        public float Predicted;
        public float Error;
    }
}