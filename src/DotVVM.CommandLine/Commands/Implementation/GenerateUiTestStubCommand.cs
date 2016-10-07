using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.CommandLine.Metadata;
using DotVVM.CommandLine.ProjectSystem;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Tools.SeleniumGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotVVM.CommandLine.Commands.Implementation
{
    public class GenerateUiTestStubCommand : CommandBase
    {
        public override string Name => "Generate UI Test Stub";

        public override string Usage => "dotvvm gen uitest <NAME>\ndotvvm gut <NAME>";

        public override bool CanHandle(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
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
                dotvvmProjectMetadata.UITestProjectPath = Helpers.AskForValue($"Enter the path to the test project\n(relative to DotVVM project directory, e.g. '..\\{dotvvmProjectMetadata.ProjectName}.Tests'): ");
            }
            var testProjectDirectory = dotvvmProjectMetadata.GetUITestProjectFullPath();
            if (!Directory.Exists(testProjectDirectory))
            {
                throw new Exception($"The directory {testProjectDirectory} doesn't exist!");
            }

            // make sure we know the test project namespace
            if (string.IsNullOrEmpty(dotvvmProjectMetadata.UITestProjectRootNamespace))
            {
                var csprojService = new CSharpProjectService();
                var csproj = csprojService.FindCsprojInDirectory(testProjectDirectory);
                if (csproj != null)
                {
                    csprojService.Load(csproj);
                    dotvvmProjectMetadata.UITestProjectRootNamespace = csprojService.GetRootNamespace();
                }
                else
                {
                    dotvvmProjectMetadata.UITestProjectRootNamespace = Helpers.AskForValue("Enter the test project root namespace: ");
                    if (string.IsNullOrEmpty(dotvvmProjectMetadata.UITestProjectRootNamespace))
                    {
                        throw new Exception("The test project root namespace must not be empty!");
                    }
                }
            }

            // generate the test stubs
            var name = args[0];
            var files = ExpandFileNames(name);

            foreach (var file in files)
            {
                Console.WriteLine($"Generating stub for {file}...");

                // determine full type name and target file
                var relativePath = Helpers.GetDothtmlFileRelativePath(dotvvmProjectMetadata, file);
                var relativeTypeName = Helpers.TrimFileExtension(relativePath) + "Helper";
                var fullTypeName = dotvvmProjectMetadata.UITestProjectRootNamespace + "." + Helpers.CreateTypeNameFromPath(relativeTypeName);
                var targetFileName = Path.Combine(dotvvmProjectMetadata.UITestProjectPath, "Helpers", relativeTypeName + ".cs");

                // generate the file
                var generator = new SeleniumHelperGenerator();
                var config = new SeleniumGeneratorConfiguration()
                {
                    TargetNamespace = Helpers.GetNamespaceFromFullType(fullTypeName),
                    HelperName = Helpers.GetTypeNameFromFullType(fullTypeName),
                    HelperFileFullPath = targetFileName,
                    ViewFullPath = file
                };

                generator.ProcessMarkupFile(DotvvmConfiguration.CreateDefault(), config);
            }
        }
    }
}
