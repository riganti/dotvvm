using System;
using System.Diagnostics;
using System.IO;
using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.CommandLine.Core.Metadata;
using DotVVM.CommandLine.Core.Templates;
using DotVVM.Compiler;
using Newtonsoft.Json;

namespace DotVVM.CommandLine.Commands.Logic.SeleniumGenerator
{
    public class SeleniumGeneratorLauncher
    {
        private const string PageObjectsText = "PageObjects";

        public static void Start(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            ResolveTestProject(dotvvmProjectMetadata);

            var metadata = JsonConvert.SerializeObject(JsonConvert.SerializeObject(dotvvmProjectMetadata));
            var exited = false;

            var processArgs = $"{CompilerConstants.Arguments.JsonOptions} {metadata} ";
            var i = 0;
            while (args[i] != null)
            {
                processArgs += $"{args[i]} ";
                i++;
            }

            var processInfo = new ProcessStartInfo(@"C:\Users\filipkalous\source\repos\dotvvm-selenium-generator.git\src\DotVVM.Framework.Tools.SeleniumGenerator\bin\Debug\netcoreapp2.0\DotVVM.Framework.Tools.SeleniumGenerator.exe") {
                RedirectStandardError = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false,
                Arguments = processArgs
            };

            var process = new Process {
                StartInfo = processInfo
            };

            process.OutputDataReceived += (sender, eventArgs) => {
                if (eventArgs?.Data?.StartsWith("#$") ?? false)
                {
                    exited = true;
                }

                Console.WriteLine(eventArgs?.Data);
            };

            process.ErrorDataReceived += (sender, eventArgs) => {
                if (eventArgs?.Data?.StartsWith("#$") ?? false)
                {
                    exited = true;
                }

                Console.WriteLine(eventArgs?.Data);
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            while (!exited)
            {
            }
        }

        private static void ResolveTestProject(DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            var metadataService = new DotvvmProjectMetadataService();

            // make sure the test directory exists
            if (string.IsNullOrEmpty(dotvvmProjectMetadata.UITestProjectPath))
            {
                var hintProjectName = $"..\\{dotvvmProjectMetadata.ProjectName}.Tests";
                dotvvmProjectMetadata.UITestProjectPath = ConsoleHelpers.AskForValue(
                    $"Enter the path to the test project\n(relative to DotVVM project directory, e.g. '{hintProjectName}'): ",
                    hintProjectName);
            }

            var testProjectDirectory = dotvvmProjectMetadata.GetUITestProjectFullPath();
            if (!Directory.Exists(testProjectDirectory))
            {
                GenerateTestProject(testProjectDirectory);
            }

            // make sure we know the test project namespace
            if (string.IsNullOrEmpty(dotvvmProjectMetadata.UITestProjectRootNamespace))
            {
                dotvvmProjectMetadata.UITestProjectRootNamespace = Path.GetFileName(testProjectDirectory);
            }

            // save the metadata
            metadataService.Save(dotvvmProjectMetadata);
        }

        private static void GenerateTestProject(string projectDirectory)
        {
            var projectFileName = Path.GetFileName(projectDirectory);
            var testProjectPath = Path.Combine(projectDirectory, projectFileName + ".csproj");
            var fileContent = GetProjectFileTextContent();

            FileSystemHelpers.WriteFile(testProjectPath, fileContent);

            CreatePageObjectsDirectory(projectDirectory);
        }

        private static void CreatePageObjectsDirectory(string projectDirectory)
        {
            var objectsDirectory = Path.Combine(projectDirectory, PageObjectsText);
            if (!Directory.Exists(objectsDirectory))
            {
                Directory.CreateDirectory(objectsDirectory);
            }
        }

        private static string GetProjectFileTextContent()
        {
            var projectTemplate = new TestProjectTemplate();

            return projectTemplate.TransformText();
        }
    }
}
