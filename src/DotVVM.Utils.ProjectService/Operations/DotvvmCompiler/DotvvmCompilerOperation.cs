using System;
using System.IO;
using System.Text;
using DotVVM.Utils.ProjectService.Extensions;
using DotVVM.Utils.ProjectService.Output;
using DotVVM.Utils.ProjectService.Output.Statistics;

namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
{
    public abstract class DotvvmCompilerOperation : CommandOperationBase
    {
        protected IStatisticsProvider StatisticsProvider { get; }
        protected string CompilerPath { get; set; }
        public sealed override string OperationName { get; set; } = "DotvvmCompilation";

        protected DotvvmCompilerOperation(IStatisticsProvider statisticsProvider, string compilerPath)
        {
            StatisticsProvider = statisticsProvider;
            CompilerPath = compilerPath;
        }

        public override OperationResult Execute(IResult result, IOutputLogger logger)
        {
            result.VerifyWebsiteAssemblyExistence();

            var operationResult = new OperationResult()
            {
                OperationName = OperationName
            };

            CompilerPath = CopyCompilerToTempFolder();
            CreateAssemblyBindings(result);

            string jsonArgument = GetJsonArgument(result);
            operationResult.Executed = true;
            logger.WriteInfo($"Starting dotvvm compilation of project {result.CsprojFullName}");
            var arguments = $"{GetLogFileArgument(result)} {jsonArgument}";

            operationResult.Successful = RunCompiler(logger, result, arguments);

            return operationResult;
        }

        private void CreateAssemblyBindings(IResult result)
        {
            new AssemblyPreprocessorFactory().GetAssemblyPreprocessor(result, CompilerPath).CreateBindings();
        }

        private string CopyCompilerToTempFolder()
        {
            var tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.TempPath,
                Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            CopyDirectory(Path.GetDirectoryName(CompilerPath), tempPath);
            return Path.Combine(tempPath, Path.GetFileName(CompilerPath));
        }

        private void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            foreach (string dir in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(Path.Combine(targetDirectory, dir.Substring(sourceDirectory.Length + 1)));
            }

            foreach (string fileName in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                File.Copy(fileName, Path.Combine(targetDirectory, fileName.Substring(sourceDirectory.Length + 1)));
            }
        }

        public abstract bool RunCompiler(IOutputLogger logger, IResult result, string arguments);

        protected string GetLogFileArgument(IResult result)
        {
            return StatisticsProvider.GetDotvvmCompilerLogFileArgument(result);
        }

        protected string GetJsonArgument(IResult result)
        {
            var json = new StringBuilder();
            json.Append("{\"WebSiteAssembly\":");
            json.Append($"\"{result.GetWebsiteAssemblyPath()}\"");
            json.Append(",\"WebSitePath\":");
            json.Append($"\"{result.GetWebsiteRootPath()}\"");
            json.Append(",\"FullCompile\":false");
            json.Append(",\"SerializeConfig\":true");
            json.Append(",\"DothtmlFiles\":[]");
            json.Append(",\"OutputResolvedDothtmlMap\":true");
            json.Append(",\"CheckBindingErrors\":false}");
            return $"--json \"{json.ToString().Escape()}\"";
        }
    }
}