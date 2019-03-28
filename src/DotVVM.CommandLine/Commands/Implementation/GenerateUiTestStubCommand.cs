using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.CommandLine.Commands.Templates;
using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Metadata;
using DotVVM.CommandLine.Security;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using DotVVM.Framework.Tools.SeleniumGenerator;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class GenerateUiTestStubCommand : CommandBase
    {
        public override string Name => "Generate UI Test Stub";

        public override string Usage => "dotvvm gen uitest <NAME>\ndotvvm gut <NAME>";

        private const string PageObjectsText = "PageObjects";

        public override bool TryConsumeArgs(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            if (string.Equals(args[0], "gen", StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(args[1], "uitest", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(2);
                return true;
            }

            if (string.Equals(args[0], "gut", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Consume(1);
                return true;
            }

            return false;
        }

        public override void Handle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            // make sure the test directory exists
            if (string.IsNullOrEmpty(dotvvmProjectMetadata.UITestProjectPath))
            {
                var hintProjectName = $"..\\{dotvvmProjectMetadata.ProjectName}.Tests";
                dotvvmProjectMetadata.UITestProjectPath = ConsoleHelpers.AskForValue($"Enter the path to the test project\n(relative to DotVVM project directory, e.g. '{hintProjectName}'): ", hintProjectName);
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

            var controlFiles = GetUserControlFiles(dotvvmProjectMetadata);
            var viewFiles = GetViewsFiles(args);

            GeneratePageObjects(dotvvmProjectMetadata, controlFiles, viewFiles);
        }

        private void GeneratePageObjects(DotvvmProjectMetadata dotvvmProjectMetadata,
            IEnumerable<string> controlsFiles,
            IEnumerable<string> viewsFiles)
        {
            var generator = new SeleniumPageObjectGenerator();

            var allFiles = controlsFiles.Concat(viewsFiles);

            foreach (var file in allFiles)
            {
                Console.WriteLine($@"Generating stub for {file}...");

                // determine full type name and target file
                var relativePath = PathHelpers.GetDothtmlFileRelativePath(dotvvmProjectMetadata, file);
                var relativeTypeName = $"{PathHelpers.TrimFileExtension(relativePath)}PageObject";
                var fullTypeName =
                    $"{dotvvmProjectMetadata.UITestProjectRootNamespace}.{PageObjectsText}.{PathHelpers.CreateTypeNameFromPath(relativeTypeName)}";
                var targetFileName = Path.Combine(dotvvmProjectMetadata.UITestProjectPath, PageObjectsText,
                    relativeTypeName + ".cs");

                var config = GetSeleniumGeneratorConfiguration(fullTypeName, targetFileName, file);

                GeneratePageObject(generator, config);
            }
        }

        private void GeneratePageObject(SeleniumPageObjectGenerator generator, SeleniumGeneratorConfiguration config)
        {
            generator.ProcessMarkupFile(DotvvmConfiguration
                    .CreateDefault(services => services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>()),
                                   config);
        }

        private IEnumerable<string> GetUserControlFiles(DotvvmProjectMetadata dotvvmProjectMetadata)
            => Directory.GetFiles(dotvvmProjectMetadata.ProjectDirectory, "*.dotcontrol", SearchOption.AllDirectories);

        private IEnumerable<string> GetViewsFiles(Arguments args)
            => ExpandFileNames(args[0]);

        private SeleniumGeneratorConfiguration GetSeleniumGeneratorConfiguration(string fullTypeName,
            string targetFileName, string file)
        {
            var config = new SeleniumGeneratorConfiguration() {
                TargetNamespace = PathHelpers.GetNamespaceFromFullType(fullTypeName),
                PageObjectName = PathHelpers.GetTypeNameFromFullType(fullTypeName),
                PageObjectFileFullPath = targetFileName,
                ViewFullPath = file
            };
            return config;
        }

        private void GenerateTestProject(string projectDirectory)
        {
            var projectFileName = Path.GetFileName(projectDirectory);
            var testProjectPath = Path.Combine(projectDirectory, projectFileName + ".csproj");
            var fileContent = GetProjectFileTextContent();

            FileSystemHelpers.WriteFile(testProjectPath, fileContent);

            CreatePageObjectsDirectory(projectDirectory);
        }

        private void CreatePageObjectsDirectory(string projectDirectory)
        {
            var objectsDirectory = Path.Combine(projectDirectory, PageObjectsText);
            if (!Directory.Exists(objectsDirectory))
            {
                Directory.CreateDirectory(objectsDirectory);
            }
        }

        private string GetProjectFileTextContent()
        {
            var projectTemplate = new TestProjectTemplate();

            return projectTemplate.TransformText();
        }
    }
}
