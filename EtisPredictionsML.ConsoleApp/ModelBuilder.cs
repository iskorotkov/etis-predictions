using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EtisPredictionsML.Model;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EtisPredictionsML.ConsoleApp
{
    public static class ModelBuilder
    {
        private static readonly MLContext MlContext = new MLContext(1);

        public static void CreateModel(string trainDataFilepath, string modelFilepath)
        {
            var trainingDataView = MlContext.Data.LoadFromTextFile<ModelInput>(trainDataFilepath,
                hasHeader: true, separatorChar: ',', allowQuoting: true, allowSparse: false);

            var trainingPipeline = BuildTrainingPipeline(MlContext);
            var mlModel = TrainModel(trainingDataView, trainingPipeline);
            Evaluate(MlContext, trainingDataView, trainingPipeline);

            SaveModel(MlContext, mlModel, modelFilepath, trainingDataView.Schema);
        }

        public static IEstimator<ITransformer> BuildTrainingPipeline(MLContext mlContext, double learningRate = 0.002D)
        {
            var dataProcessPipeline = mlContext.Transforms.Concatenate("Features", "X1", "X2", "X3", "X4", "X5", "X6",
                "X7", "X8", "X9", "X10", "X11", "X12", "X13", "X14", "X15", "X16", "X17", "X18", "X19", "X20", "X21",
                "X22", "X23", "X24", "X25", "X26", "X27", "X28", "X29", "X30", "X31", "X32", "X33", "X34", "X35", "X36",
                "X37", "X38", "X39", "X40", "X41", "X42", "X43", "X44", "X45", "X46", "X47", "X48", "X49", "X50", "X51",
                "X52", "X53");

            var trainer = mlContext.Regression.Trainers.Gam("D1", learningRate: learningRate);
            return dataProcessPipeline.Append(trainer);
        }

        public static ITransformer TrainModel(IDataView trainingDataView, IEstimator<ITransformer> trainingPipeline)
        {
            return trainingPipeline.Fit(trainingDataView);
        }

        private static void Evaluate(MLContext mlContext, IDataView trainingDataView,
            IEstimator<ITransformer> trainingPipeline)
        {
            var crossValidationResults =
                mlContext.Regression.CrossValidate(trainingDataView, trainingPipeline, 5, "D1");
            PrintRegressionFoldsAverageMetrics(crossValidationResults);
        }

        private static void SaveModel(MLContext mlContext, ITransformer mlModel, string modelRelativePath,
            DataViewSchema modelInputSchema)
        {
            mlContext.Model.Save(mlModel, modelInputSchema, GetAbsolutePath(modelRelativePath));
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var assemblyFolderPath = dataRoot!.Directory!.FullName;
            return Path.Combine(assemblyFolderPath, relativePath);
        }

        public static void PrintRegressionFoldsAverageMetrics(
            IEnumerable<TrainCatalogBase.CrossValidationResult<RegressionMetrics>> crossValidationResults)
        {
            var validationResults = crossValidationResults.ToArray();
            var l1 = validationResults.Select(r => r.Metrics.MeanAbsoluteError);
            var l2 = validationResults.Select(r => r.Metrics.MeanSquaredError);
            var rms = validationResults.Select(r => r.Metrics.RootMeanSquaredError);
            var lossFunction = validationResults.Select(r => r.Metrics.LossFunction);
            var r2 = validationResults.Select(r => r.Metrics.RSquared);

            Console.WriteLine($"Average L1 Loss: {l1.Average():0.###}");
            Console.WriteLine($"Average L2 Loss: {l2.Average():0.###}");
            Console.WriteLine($"Average RMS: {rms.Average():0.###}");
            Console.WriteLine($"Average Loss Function: {lossFunction.Average():0.###}");
            Console.WriteLine($"Average R-squared: {r2.Average():0.###}");
        }
    }
}
