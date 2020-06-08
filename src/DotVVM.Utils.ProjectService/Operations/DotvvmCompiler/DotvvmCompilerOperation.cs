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

        public override OperationResult Execute(IResolvedProjectMetadata metadata, IOutputLogger logger)
        {
            metadata.VerifyWebsiteAssemblyExistence();

            var operationResult = new OperationResult()
            {
                OperationName = OperationName
            };

            CompilerPath = CopyCompilerToTempFolder();
            CreateAssemblyBindings(metadata);

            string jsonArgument = GetJsonArgument(metadata);
            operationResult.Executed = true;
            logger.WriteInfo($"Starting dotvvm compilation of project {metadata.CsprojFullName}");
            var arguments = $"{GetLogFileArgument(metadata)} {jsonArgument}";

            operationResult.Successful = RunCompiler(logger, metadata, arguments);

            return operationResult;
        }

        private void CreateAssemblyBindings(IResolvedProjectMetadata metadata)
        {
            new AssemblyPreprocessorFactory().GetAssemblyPreprocessor(metadata, CompilerPath).CreateBindings();
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

        public abstract bool RunCompiler(IOutputLogger logger, IResolvedProjectMetadata metadata, string arguments);

        protected string GetLogFileArgument(IResolvedProjectMetadata metadata)
        {
            return StatisticsProvider.GetDotvvmCompilerLogFileArgument(metadata);
        }

        protected string GetJsonArgument(IResolvedProjectMetadata metadata)
        {
            var json = new StringBuilder();
            json.Append("{\"WebSiteAssembly\":");
            json.Append($"\"{metadata.GetWebsiteAssemblyPath()}\"");
            json.Append(",\"WebSitePath\":");
            json.Append($"\"{metadata.GetWebsiteRootPath()}\"");
            json.Append(",\"FullCompile\":false");
            json.Append(",\"SerializeConfig\":true");
            json.Append(",\"DothtmlFiles\":[]");
            json.Append(",\"OutputResolvedDothtmlMap\":true");
            json.Append(",\"CheckBindingErrors\":false}");
            return $"--json \"{json.ToString().Escape()}\"";
        }
    }
}