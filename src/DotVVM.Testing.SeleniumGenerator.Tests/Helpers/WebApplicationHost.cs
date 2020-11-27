using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using DotVVM.CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DotVVM.Testing.SeleniumGenerator.Tests.Helpers
{
    public class WebApplicationHost : IDisposable
    {
        private readonly TestContext testContext;
        private readonly string webApplicationTemplatePath;
        private bool initialized;
        private readonly string workingDirectory;
        private readonly string webAppDirectory;
        private readonly string testProjectName;
        private readonly string testProjectCsproj;
        private readonly string dotvvmJsonPath;
        private ProjectMetadataOld metadata;

        public string TestProjectDirectory { get; private set; }


        public WebApplicationHost(TestContext testContext, string webApplicationTemplatePath)
        {
            this.testContext = testContext;
            this.webApplicationTemplatePath = webApplicationTemplatePath;

            workingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var webAppName = Path.GetFileName(webApplicationTemplatePath);
            webAppDirectory = Path.Combine(workingDirectory, webAppName);
            dotvvmJsonPath = Path.Combine(webAppDirectory, ".dotvvm.json");

            testProjectName = webAppName + ".Tests";
            TestProjectDirectory = Path.GetFullPath(Path.Combine(webAppDirectory, "..", testProjectName));
            testProjectCsproj = Path.Combine(TestProjectDirectory, testProjectName + ".csproj");
        }

        public void Initialize()
        {
            initialized = true;

            // prepare temp directories
            Directory.CreateDirectory(workingDirectory);
            Directory.CreateDirectory(webAppDirectory);

            // copy application in the working directory
            Process.Start("xcopy", $"/E \"{webApplicationTemplatePath}\" \"{webAppDirectory}\"")?.WaitForExit();

            // set test project path in .dotvvm.json
            metadata = DotvvmProject.GetProjectMetadata(new FileInfo(dotvvmJsonPath)).GetAwaiter().GetResult();
            metadata = metadata.WithUITestProject($"../{testProjectName}", testProjectName);

            // change current directory
            Environment.CurrentDirectory = webAppDirectory;
        }

        public string ProcessMarkupFile(string markupFilePath)
        {
            if (!initialized)
            {
                Initialize();
            }

            throw new NotImplementedException("Reimplement WebApplicationHost.ProcessMarkupFile with DotVVM.CommandLine.");


            return File.ReadAllText(markupFilePath);
        }

        private void StartGeneratorProcess(Process process)
        {
            var exited = false;
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

            while (!exited && !process.HasExited)
            {
            }

            if (process.ExitCode != 0)
            {
                throw new Exception("Selenium generation failed.");
            }
        }

        internal void FixReferencedProjectPath(string proxiesCsProjPath)
        {
            // TODO: remove this when we replace the proxies with NuGet package
            var csproj = File.ReadAllText(testProjectCsproj);
            csproj = csproj.Replace("..\\DotVVM.Framework.Testing.SeleniumHelpers.csproj", proxiesCsProjPath);
            File.WriteAllText(testProjectCsproj, csproj);
        }

        public async Task<Compilation> CompileAsync()
        {
            var manager = new AnalyzerManager();
            var project = manager.GetProject(testProjectCsproj);

            var workspace = new AdhocWorkspace();
            var roslynProject = project.AddToWorkspace(workspace);
            var compilation = await roslynProject.GetCompilationAsync();

            var diagnostics = compilation.GetDiagnostics();
            foreach (var diagnostic in diagnostics)
            {
                testContext.WriteLine(diagnostic.GetMessage());
            }

            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                Assert.Fail("Test project build failed!");
            }

            return compilation;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(this.workingDirectory, true);
            }
            catch (IOException)
            {
            }
        }

    }
}
