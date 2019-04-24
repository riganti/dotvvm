using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;
using System.Linq;
using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.CommandLine.Core.Metadata;
using Newtonsoft.Json;
using System.Collections.Generic;
using DotVVM.Utils.ConfigurationHost.Initialization;
using System.Reflection;
using DotVVM.CommandLine.Core.Templates;
using DotVVM.Framework.Tools.SeleniumGenerator.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class Program
    {
        private const string PageObjectsText = "PageObjects";

        public static void Main(string[] args)
        {
            try
            {
                var arguments = new Arguments(args);

                DotvvmProjectMetadata dotvvmProjectMetadata = null;
                if (string.Equals(arguments[0], "--json", StringComparison.CurrentCultureIgnoreCase))
                {
                    dotvvmProjectMetadata = JsonConvert.DeserializeObject<DotvvmProjectMetadata>(args[1]);
                    dotvvmProjectMetadata.WebAssemblyPath = dotvvmProjectMetadata.WebAssemblyPath.Replace(@"\\", @"\");
                    dotvvmProjectMetadata.ProjectDirectory = dotvvmProjectMetadata.ProjectDirectory.Replace(@"\\", @"\");
                    dotvvmProjectMetadata.MetadataFilePath = dotvvmProjectMetadata.MetadataFilePath.Replace(@"\\", @"\");
                    arguments.Consume(2);
                }
                else
                {
                    Console.WriteLine(@"Provide correct metadata.");
                    Environment.Exit(1);
                }

                ResolveTestProject(dotvvmProjectMetadata);
                CreatePageObjectsDirectory(dotvvmProjectMetadata.GetUITestProjectFullPath());

                var config = ConfigurationHost.InitDotVVM(Assembly.LoadFile(dotvvmProjectMetadata.WebAssemblyPath),
                    dotvvmProjectMetadata.ProjectDirectory,
                    services => services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>());

                // generate the test stubs
                GeneratePageObjects(dotvvmProjectMetadata, config, arguments);

                Console.WriteLine(@"#$ Exit 0 - DotVVM Selenium Generator Ended");
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(@"#$ Exit 1 - DotVVM Selenium Generator Failed" + e);
                throw;
            }
        }

        private static void GeneratePageObjects(DotvvmProjectMetadata dotvvmProjectMetadata,
            DotvvmConfiguration dotvvmConfig, 
            Arguments arguments)
        {
            var options = dotvvmConfig.ServiceProvider.GetService<SeleniumGeneratorOptions>();
            var generator = new SeleniumPageObjectGenerator(options, dotvvmConfig);

            IEnumerable<string> controlFiles = new List<string>();
            IEnumerable<string> viewFiles;

            if (arguments[0] != null)
            {
                viewFiles = GetViewsFiles(new[] { arguments[0] });
            }
            else
            {
                // generate all views and user controls files if no argument was specified
                viewFiles = dotvvmConfig.RouteTable.Where(b => b.VirtualPath != null).Select(r => r.VirtualPath);
                controlFiles = dotvvmConfig.Markup.Controls.Where(b => b.Src != null).Select(c => c.Src);
            }

            var allFiles = controlFiles.Concat(viewFiles).Distinct();

            foreach (var file in allFiles)
            {
                if (File.Exists(file))
                {
                    Console.WriteLine($"Generating stub for {file}...");

                    // determine full type name and target file
                    var relativePath = PathHelpers.GetDothtmlFileRelativePath(dotvvmProjectMetadata, file);
                    var relativeTypeName = $"{PathHelpers.TrimFileExtension(relativePath)}PageObject";
                    var fullTypeName = $"{dotvvmProjectMetadata.UITestProjectRootNamespace}.{PageObjectsText}.{PathHelpers.CreateTypeNameFromPath(relativeTypeName)}";
                    var targetFileName = Path.Combine(dotvvmProjectMetadata.UITestProjectPath, PageObjectsText, relativeTypeName + ".cs");

                    var config = GetSeleniumGeneratorConfiguration(fullTypeName, targetFileName, file);

                    GeneratePageObject(generator, config);
                }
            }
        }

        private static void GeneratePageObject(SeleniumPageObjectGenerator generator,
            SeleniumGeneratorConfiguration config)
           => generator.ProcessMarkupFile(config);

        private static IEnumerable<string> GetViewsFiles(IEnumerable<string> filePaths)
        {
            return filePaths.Select(ExpandFileName);
        }

        private static SeleniumGeneratorConfiguration GetSeleniumGeneratorConfiguration(string fullTypeName,
            string targetFileName, string file)
        {
            return new SeleniumGeneratorConfiguration()
            {
                TargetNamespace = PathHelpers.GetNamespaceFromFullType(fullTypeName),
                PageObjectName = PathHelpers.GetTypeNameFromFullType(fullTypeName),
                PageObjectFileFullPath = targetFileName,
                ViewFullPath = file
            };
        }

        protected static string ExpandFileName(string name)
        {
            // TODO: add wildcard support
            return Path.GetFullPath(name);
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
                GenerateTestProject(testProjectDirectory, dotvvmProjectMetadata.ProjectDirectory);
            }

            // make sure we know the test project namespace
            if (string.IsNullOrEmpty(dotvvmProjectMetadata.UITestProjectRootNamespace))
            {
                dotvvmProjectMetadata.UITestProjectRootNamespace = Path.GetFileName(testProjectDirectory);
            }

            // save the metadata
            metadataService.Save(dotvvmProjectMetadata);
        }

        private static void GenerateTestProject(string testProjectDirectory, string projectDirectory)
        {
            var testProjectFileName = Path.GetFileName(testProjectDirectory);
            var webProjectFileName = Path.GetFileName(projectDirectory);
            var testProjectPath = Path.Combine(testProjectDirectory, testProjectFileName + ".csproj");
            var webProjectPath = Path.Combine(projectDirectory, webProjectFileName + ".csproj");
            var fileContent = GetProjectFileTextContent(webProjectPath);

            FileSystemHelpers.WriteFile(testProjectPath, fileContent);
        }

        private static void CreatePageObjectsDirectory(string projectDirectory)
        {
            var objectsDirectory = Path.Combine(projectDirectory, PageObjectsText);
            if (!Directory.Exists(objectsDirectory))
            {
                Directory.CreateDirectory(objectsDirectory);
            }
        }

        private static string GetProjectFileTextContent(string projectDirectory)
        {
            var projectTemplate = new TestProjectTemplate
            {
                WebProjectPath = projectDirectory
            };

            return projectTemplate.TransformText();
        }
    }
}
