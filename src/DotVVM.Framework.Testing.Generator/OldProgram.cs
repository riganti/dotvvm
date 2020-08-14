using DotVVM.Framework.Configuration;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.IO;
using System.Linq;
using DotVVM.CommandLine.Core;
using DotVVM.CommandLine.Core.Arguments;
using DotVVM.Cli;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using DotVVM.Utils.ConfigurationHost.Initialization;
using System.Reflection;
using System.Threading;
using DotVVM.CommandLine.Core.Templates;
using DotVVM.Framework.Testing.Generator;
using DotVVM.Framework.Tools.SeleniumGenerator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class OldProgram
    {
        private const string PageObjectsText = "PageObjects";


        private static void WaitForDebugger(bool _break = false)
        {
            Console.WriteLine("Process ID: " + Process.GetCurrentProcess().Id);
            while (!Debugger.IsAttached) Thread.Sleep(32);
            if (_break)
            {
                Debugger.Break();
            }
        }

        public static void Main(string[] args)
        {
            try
            {
                var arguments = new Arguments(args);
                if (arguments[0] == Compiler.CompilerConstants.Arguments.WaitForDebuggerAndBreak)
                {
                    arguments.Consume(1);
                    WaitForDebugger(true);
                }
                if (arguments[0] == Compiler.CompilerConstants.Arguments.WaitForDebugger)
                {
                    arguments.Consume(1);
                    WaitForDebugger();
                }

                ProjectMetadataJson dotvvmProjectMetadata = null;
                if (string.Equals(arguments[0], "--json", StringComparison.CurrentCultureIgnoreCase))
                {
                    dotvvmProjectMetadata = JsonConvert.DeserializeObject<DotvvmProjectMetadata>(arguments[1]);
                    dotvvmProjectMetadata.WebAssemblyPath = dotvvmProjectMetadata.WebAssemblyPath?.Replace(@"\\", @"\");
                    dotvvmProjectMetadata.ProjectDirectory = dotvvmProjectMetadata.ProjectDirectory?.Replace(@"\\", @"\");
                    dotvvmProjectMetadata.MetadataFilePath = dotvvmProjectMetadata.MetadataFilePath?.Replace(@"\\", @"\");
                    dotvvmProjectMetadata.UITestProjectPath = dotvvmProjectMetadata.UITestProjectPath?.Replace(@"\\", @"\");
                    arguments.Consume(2);
                }
                else
                {
                    ExitProgram(1, "Parameter --json <data> is missing.");
                }

                ResolveTestProject(dotvvmProjectMetadata);
                CreatePageObjectsDirectory(dotvvmProjectMetadata.GetUITestProjectFullPath());

                var config = ConfigurationHost.InitDotVVM(Assembly.LoadFile(dotvvmProjectMetadata.WebAssemblyPath),
                    dotvvmProjectMetadata.ProjectDirectory,
                    services => services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>());

                // generate the test stubs
                GeneratePageObjects(dotvvmProjectMetadata, config, arguments);
                ExitProgram(0, "DotVVM Selenium Generator Ended");

            }
            catch (Exception e)
            {
                Console.WriteLine(@"#$ Exit 1 - DotVVM Selenium Generator Failed" + e);
                throw;
            }
        }

        private static void ExitProgram(int code, string message = "")
        {
            Console.WriteLine($@"#$ Exit {code} - {message}");
            Environment.Exit(code);
        }

        private static void GeneratePageObjects(ProjectMetadataJson dotvvmProjectMetadata, DotvvmConfiguration dotvvmConfig, Arguments arguments)
        {
            var options = PrepareSeleniumGeneratorOptions(dotvvmConfig);
            var generator = new SeleniumPageObjectGenerator(options, dotvvmConfig);

            IEnumerable<string> controlFiles = new List<string>();
            IEnumerable<string> viewFiles;

            if (arguments[0] != null)
            {

                var parsedArguments = SplitArguments(arguments);
                if (parsedArguments.Any())
                {
                    viewFiles = GetViewsFiles(parsedArguments);
                }
                else
                {
                    GetAllViewsAndControlsInProject(dotvvmConfig, out controlFiles, out viewFiles);
                }
            }
            else
            {
                GetAllViewsAndControlsInProject(dotvvmConfig, out controlFiles, out viewFiles);
            }

            var allFiles = controlFiles.Concat(viewFiles).Distinct();
            var configurations = new List<SeleniumGeneratorConfiguration>();
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

                    configurations.Add(GetSeleniumGeneratorConfiguration(fullTypeName, targetFileName, file));
                }
            }
            generator.ProcessMarkupFiles(configurations).GetAwaiter().GetResult();
        }

        private static void GetAllViewsAndControlsInProject(DotvvmConfiguration dotvvmConfig, out IEnumerable<string> controlFiles, out IEnumerable<string> viewFiles)
        {
            // generate all views and user controls files if no argument was specified
            viewFiles = dotvvmConfig.RouteTable.Where(b => b.VirtualPath != null).Select(r => r.VirtualPath);
            controlFiles = dotvvmConfig.Markup.Controls.Where(b => b.Src != null).Select(c => c.Src);
        }

        private static SeleniumGeneratorOptions PrepareSeleniumGeneratorOptions(DotvvmConfiguration dotvvmConfig)
        {
            var options = dotvvmConfig.ServiceProvider.TryGetService<SeleniumGeneratorOptions>();
            if (options == null)
            {
                ExitProgram(1, "Cannot find any SeleniumGeneratorOptions. Please register DotVVM.Testing.SeleniumHelpers to your web project.");
            }
            options.AddAssembly(typeof(Program).Assembly);
            return options;
        }

        private static IEnumerable<string> SplitArguments(Arguments arguments)
        {
            var i = 0;
            var parsedArguments = new List<string>();
            while (arguments[i] != null)
            {
                if (arguments[i].StartsWith("-"))
                {
                    i++;
                    continue;
                }
                parsedArguments.Add(arguments[i]);
                i++;
            }
            return parsedArguments;
        }

        private static async Task GeneratePageObject(SeleniumPageObjectGenerator generator, SeleniumGeneratorConfiguration config) => await generator.ProcessMarkupFile(config);

        private static IEnumerable<string> GetViewsFiles(IEnumerable<string> filePaths)
        {
            return filePaths.Select(ExpandFileName);
        }

        private static SeleniumGeneratorConfiguration GetSeleniumGeneratorConfiguration(string fullTypeName,
            string targetFileName, string file)
        {
            return new SeleniumGeneratorConfiguration() {
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

        private static void ResolveTestProject(ProjectMetadataJson dotvvmProjectMetadata)
        {
            var metadataService = new DotvvmProjectMetadataService();

            if (string.IsNullOrEmpty(dotvvmProjectMetadata.UITestProjectPath))
            {
                var hintProjectName = $"..\\{dotvvmProjectMetadata.ProjectName}.Tests";

                if (!Console.IsInputRedirected)
                {
                    dotvvmProjectMetadata.UITestProjectPath =
                        ConsoleHelpers.AskForValue($"Enter the path to the test project\n(relative to DotVVM project directory, e.g. '{hintProjectName}'): ",
                        hintProjectName);
                }
                else
                {
                    dotvvmProjectMetadata.UITestProjectPath = hintProjectName;
                }

                Console.WriteLine($@"Path to test project is set to ""{dotvvmProjectMetadata.UITestProjectPath}""");
            }

            // make sure the test directory exists
            var testProjectDirectory = dotvvmProjectMetadata.GetUITestProjectFullPath();
            Console.WriteLine($"# Directory '{testProjectDirectory}' already exists. Skipping test project scaffolding.");
            if (!Directory.Exists(testProjectDirectory))
            {
                var relativeWebDirectory = new Uri(testProjectDirectory)
                    .MakeRelativeUri(new Uri(dotvvmProjectMetadata.ProjectDirectory))
                    .ToString();
                relativeWebDirectory = Uri.UnescapeDataString(relativeWebDirectory);
                GenerateTestProject(testProjectDirectory, relativeWebDirectory);
            }

            // make sure we know the test project namespace
            if (string.IsNullOrEmpty(dotvvmProjectMetadata.UITestProjectRootNamespace))
            {
                dotvvmProjectMetadata.UITestProjectRootNamespace = Path.GetFileName(testProjectDirectory);
            }

            // save the metadata
            metadataService.Save(dotvvmProjectMetadata);
        }

        private static void GenerateTestProject(string testProjectDirectory, string relativeWebDirectory)
        {
            var testProjectFileName = Path.GetFileName(testProjectDirectory);
            var testProjectPath = Path.Combine(testProjectDirectory, testProjectFileName + ".csproj");
            var webProjectPath = Path.Combine(relativeWebDirectory + ".csproj");
            var seleniumHelpersVersion = "2.3.0-preview01-43572-selenium-generator";
            var isNetStandardApp = true;

            var fileContent = GetProjectFileTextContent(relativeWebDirectory, webProjectPath, seleniumHelpersVersion, isNetStandardApp);
            var stContent = GetSeleniumTestBase(testProjectFileName);

            FileSystemHelpers.WriteFile(testProjectPath, fileContent);

            // Create base selenium test class
            Directory.CreateDirectory(Path.Combine(testProjectDirectory, "Core"));
            FileSystemHelpers.WriteFile(Path.Combine(testProjectDirectory, "Core\\AppSeleniumTest.cs"), stContent);

            // Create base seleniumconfig.json
            Directory.CreateDirectory(Path.Combine(testProjectDirectory, "Core"));
            FileSystemHelpers.WriteFile(Path.Combine(testProjectDirectory, "Core\\AppSeleniumTest.cs"), stContent);
        }

        private static string GetSeleniumTestBase(string @namespace)
        {
            var projectTemplate = new AppSeleniumTestTemplate() {
                Namespace = @namespace
            };
            return projectTemplate.TransformText();
        }

        private static void CreatePageObjectsDirectory(string projectDirectory)
        {
            var objectsDirectory = Path.Combine(projectDirectory, PageObjectsText);
            if (!Directory.Exists(objectsDirectory))
            {
                Directory.CreateDirectory(objectsDirectory);
            }
        }

        private static string GetProjectFileTextContent(string webDirectory, string webProjectPath, string seleniumHelpersVersion, bool isNetStandardApp)
        {
            var projectTemplate = new TestProjectTemplate {
                WebProjectPath = webDirectory,
                WebCsProjPath = webProjectPath,
                SeleniumHelpersVersion = seleniumHelpersVersion,
                IsNetstandardApp = isNetStandardApp
            };

            return projectTemplate.TransformText();
        }
    }
}
