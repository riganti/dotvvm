using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using DotVVM.CommandLine;
using Microsoft.Extensions.Logging;

namespace DotVVM.CommandLine
{
    public static class CompilerCommands
    {
        public const string ShimName = "Compiler";
        public const string AssemblyNameOption = "--assembly-name";
        public const string ApplicationPathOption = "--application-path";
        public const string PackageName = "DotVVM.Compiler";
        public const string ShimProgramFile = "Compiler.cs";
        public const string ShimProjectFile = "Compiler.csproj";
        public const string ProgramClass = "DotVVM.Compiler.Program";

        public static void AddCompilerCommands(this Command command)
        {
            var compileCmd = new Command("compile", "Invokes the DotVVM compiler");
            compileCmd.AddTargetArgument();
            compileCmd.AddArgument(new Argument<string[]>(
                name: "compilerArgs",
                description: "Arguments passed to the compiler"));
            compileCmd.AddOption(new Option<FileSystemInfo>(
                alias: "--compiler",
                description: "Path to the source of DotVVM.Compiler"));
            compileCmd.AddOption(new Option<bool>(
                alias: "--debug",
                description: "Build the compiler shim with the Debug configuration"));
            compileCmd.Handler = CommandHandler.Create(typeof(CompilerCommands).GetMethod(nameof(HandleCompile))!);
            command.AddCommand(compileCmd);
        }

        public static int HandleCompile(
            ProjectMetadata metadata,
            FileSystemInfo target,
            MSBuild msbuild,
            string[]? compilerArgs,
            FileSystemInfo? compiler,
            bool debug,
            bool msbuildOutput,
            ILogger logger)
        {
            var allArgs = new List<string>
            {
                AssemblyNameOption,
                metadata.AssemblyName,
                ApplicationPathOption,
                Path.GetDirectoryName(metadata.ProjectFilePath)
            };
            if (compilerArgs is object)
            {
                allArgs.AddRange(compilerArgs);
            }
            
            var targetFramework = Shims.GetSuitableTargetFramework(metadata.TargetFrameworks).GetShortFolderName();

            var success = Shims.TryCreateRunShim(
                shimName: ShimName,
                target: target,
                app: compiler,
                args: allArgs,
                createShim: c => Shims.CreateBasicShim(
                    context: c,
                    shimName: ShimName,
                    shimProjectFile: ShimProjectFile,
                    shimTargetFramework: targetFramework,
                    shimProgramFile: ShimProgramFile,
                    appPackage: PackageName,
                    appPackageVersion: metadata.PackageVersion,
                    appProgramClass: ProgramClass),
                isDebug: debug,
                msbuild: msbuild,
                shouldShowMSBuild: msbuildOutput,
                logger: logger);
            
            return success ? 0 : 1;
        }
    }
}
