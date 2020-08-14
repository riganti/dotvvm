using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Cli;
using DotVVM.Framework.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Testing.Generator
{
    public static class Program
    {
        public const string PageObjectDirectory = "PageObjects";

        public static async Task<bool> TryGeneratePageObjects(
            DirectoryInfo testDirectory,
            DotvvmConfiguration config,
            ProjectMetadata metadata,
            string[]? dothtmlFiles,
            bool force,
            ILogger logger)
        {
            var pageObjectsDirectory = new DirectoryInfo(Path.Combine(testDirectory.FullName, PageObjectDirectory));
            if (!pageObjectsDirectory.Exists)
            {
                pageObjectsDirectory.Create();
            }

            var options = config.ServiceProvider.GetService<SeleniumGeneratorOptions>();
            if (options is null)
            {
                logger.LogCritical("Could not find generator options. "
                    + "Please register DotVVM.Framework.Testing in your DotvvmStartup.");
                return false;
            }

            options.AddAssembly(typeof(Program).Assembly);
            var generator = new SeleniumPageObjectGenerator(options, config);

            if (dothtmlFiles is null)
            {
                var viewFiles = config.RouteTable.Where(b => b.VirtualPath != null).Select(r => r.VirtualPath);
                var controlFiles = config.Markup.Controls.Where(b => b.Src != null).Select(c => c.Src);
                dothtmlFiles = viewFiles.Concat(controlFiles).Distinct().ToArray()!;
            }

            var configurations = new List<SeleniumGeneratorConfiguration>();
            foreach (var dothtmlFile in dothtmlFiles)
            {
                var typeName = $"{Names.GetClass(Path.GetFileNameWithoutExtension(dothtmlFile))}PageObject";
                var typeNamespace = $"{metadata.UITestProjectRootNamespace}.{PageObjectDirectory}";
                var fullTypeName = $"{typeNamespace}.{typeName}";
                var file = new FileInfo(Path.Combine(pageObjectsDirectory.FullName, $"{typeName}.cs"));
                if (!file.Exists || force)
                {
                    logger.LogDebug($"Generating stub for '{dothtmlFile}'.");
                    configurations.Add(new SeleniumGeneratorConfiguration
                    {
                        TargetNamespace = typeNamespace,
                        PageObjectName = typeName,
                        PageObjectFileFullPath = file.FullName,
                        ViewFullPath = dothtmlFile
                    });
                }
            }
            await generator.ProcessMarkupFiles(configurations);
            return true;
        }

        public static async Task<int> Run(
            ProjectMetadata metadata,
            string[]? views,
            string? name,
            DirectoryInfo directory,
            bool force,
            ILogger logger)
        {
            name ??= $"{metadata.ProjectName}.Tests";
            var testDirectory = new DirectoryInfo(Path.Combine(directory.FullName, name));

            // generate a stub of the test project if necessary
            if (metadata.UITestProjectPath is null || metadata.UITestProjectRootNamespace is null)
            {
                if (testDirectory.Exists)
                {
                    if (force)
                    {
                        logger.LogInformation($"Overwriting '{testDirectory}'.");
                        testDirectory.Delete(true);
                    }
                    else
                    {
                        logger.LogCritical($"Directory '{testDirectory}' already exists. Use --force to overwrite it.");
                        return 1;
                    }
                }

                logger.LogInformation($"Generating the '{name}' project stub.");
                // TODO: Add path to the csproj to ProjectMetadata
                var projectFile = ProjectFile.FindProjectFile(new DirectoryInfo(metadata.ProjectDirectory))!;
                var testProjectFile = await UITestProject.GenerateStub(
                    webProjectPath: projectFile.FullName,
                    name: name,
                    directory: directory,
                    @namespace: name);
                if (testProjectFile is null)
                {
                    return 1;
                }
                metadata = metadata.WithUITestProject(testProjectFile.FullName, name);
                await DotvvmProject.SaveProjectMetadata(metadata);
            }

            var config = DotvvmProject.GetConfiguration(metadata.ProjectName, metadata.ProjectDirectory);

            return await TryGeneratePageObjects(testDirectory, config, metadata, views, force, logger)
                ? 0
                : 1;
        }

        public static int Main(string[] args)
        {
            var rootCmd = new RootCommand("DotVVM UI Test Generator")
            {
                Name = "dotvvm-test-generator"
            };
            rootCmd.AddOption(new Option<string>(
                aliases: new [] {"-n", "--name"},
                description: "The name of the generated UI test project"));
            rootCmd.AddOption(new Option<DirectoryInfo>(
                aliases: new [] {"-d", "--directory"},
                getDefaultValue: () => new DirectoryInfo(Environment.CurrentDirectory).Parent,
                description: "The parent directory of the generated UI test project"));
            rootCmd.AddOption(new Option<bool>(
                aliases: new [] {"-f", "--force"},
                description: "Force a regeneration of the UI test project"));
            rootCmd.AddVerboseOption();
            rootCmd.AddDebuggerBreakOption();
            rootCmd.AddTargetArgument();
            rootCmd.AddArgument(new Argument<string[]>(
                name: "views",
                description: "The views to generate tests for")
            {
                Arity = ArgumentArity.ZeroOrMore
            });
            rootCmd.Handler = CommandHandler.Create(typeof(Program).GetMethod(nameof(Run))!);

            new CommandLineBuilder(rootCmd)
                .UseDefaults()
                .UseLogging()
                .UseDebuggerBreak()
                .UseDotvvmMetadata()
                .Build();
            return rootCmd.Invoke(args);
        }
    }
}
