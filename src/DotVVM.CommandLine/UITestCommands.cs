using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using DotVVM.CommandLine;
using Microsoft.Extensions.Logging;

namespace DotVVM.CommandLine
{
    public static class UITestCommands
    {
        public const string ShimName = "UITestGenerator";
        public const string PackageName = "DotVVM.Framework.Testing.Generator";
        public const string ShimProgramFile = "UITestGenerator.cs";
        public const string ShimProjectFile = "UITestGenerator.csproj";
        public const string ProgramClass = "DotVVM.Framework.Testing.Generator.Program";

        public static void AddUITestCommands(this Command command)
        {
            var uiTestCmd = new Command("uitest", "Invokes the UI test generator.");
            uiTestCmd.AddTargetArgument();
            uiTestCmd.AddArgument(new Argument<string[]>(
                name: "generatorArgs",
                description: "Arguments passed to the generator"));
            uiTestCmd.AddOption(new Option<FileSystemInfo>(
                alias: "--generator",
                description: "Path to the source of DotVVM.Framework.Testing.Generator"));
            uiTestCmd.AddOption(new Option<bool>(
                alias: "--debug",
                description: "Build the compiler shim with the Debug configuration"));
            uiTestCmd.Handler = CommandHandler.Create(typeof(UITestCommands).GetMethod(nameof(HandleUITest))!);
            command.AddCommand(uiTestCmd);
        }

        public static int HandleUITest(
            ProjectMetadata metadata,
            FileSystemInfo target,
            string[]? generatorArgs,
            FileSystemInfo? generator,
            bool debug,
            bool msbuildOutput,
            ILogger logger)
        {
            // TODO: Add the csproj to ProjectMetadat
            var projectFile = ProjectFile.FindProjectFile(target)!;
            var allArgs = new List<string>
            {
                projectFile.FullName
            };
            if (generatorArgs is object)
            {
                allArgs.AddRange(generatorArgs);
            }

            var success = Shims.TryCreateRunShim(
                shimName: ShimName,
                target: target,
                app: generator,
                args: allArgs,
                createShim: c => Shims.CreateBasicShim(
                    context: c,
                    shimName: ShimName,
                    shimProjectFile: ShimProjectFile,
                    shimTargetFramework: Shims.Netcoreapp,
                    shimProgramFile: ShimProgramFile,
                    appPackage: PackageName,
                    appPackageVersion: metadata.PackageVersion,
                    appProgramClass: ProgramClass),
                isDebug: debug,
                shouldShowMSBuild: msbuildOutput,
                logger: logger);
            return success ? 0 : 1;
        }
    }
}
