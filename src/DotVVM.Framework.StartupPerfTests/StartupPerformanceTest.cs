using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;

namespace DotVVM.Framework.StartupPerfTests
{
    public class StartupPerformanceTest
    {
        private readonly FileInfo project;
        private readonly TestTarget type;
        private readonly int repeat;
        private readonly string url;
        private readonly bool verbose;

        public StartupPerformanceTest(FileInfo project, TestTarget type, int repeat, string url, bool verbose)
        {
            this.project = project;
            this.type = type;
            this.repeat = repeat;
            this.url = url;
            this.verbose = verbose;
        }


        public void HandleCommand()
        {
            // test project existence
            var projectPath = project.FullName;
            if (!project.Exists)
            {
                throw new Exception($"The project {projectPath} doesn't exist!");
            }

            // find a random port
            var port = NetworkingHelpers.FindRandomPort();
            var urlToTest = $"http://localhost:{port}/{url.TrimStart('/')}";

            // prepare directories
            var dir = Path.GetDirectoryName(projectPath);
            TraceOutput($"Project dir: {dir}");
            var tempDir = FileSystemHelper.CreateTempDir();
            TraceOutput($"Temp dir: {tempDir}");

            long measuredTime = 0;
            if (type == TestTarget.Owin)
            {
                // OWIN
                TraceOutput($"Publishing...");
                RunProcessAndWait(@"c:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe", @$"""{Path.GetFileName(projectPath)}"" /p:DeployOnBuild=true /t:WebPublish /p:WebPublishMethod=FileSystem /p:publishUrl=""{tempDir}""", dir);

                for (var i = 0; i < repeat; i++)
                {
                    TraceOutput($"Attempt #{i + 1}");
                    measuredTime += RunProcessAndWaitForHealthCheck(@"C:\Program Files (x86)\IIS Express\iisexpress.exe", $@"""/path:{dir}"" /port:{port}", dir, urlToTest);
                }
            }
            else if (type == TestTarget.AspNetCore)
            {
                // ASP.NET Core
                TraceOutput($"Publishing...");
                RunProcessAndWait(@"dotnet", $@"publish -c Release -o ""{tempDir}""", dir);

                for (var i = 0; i < repeat; i++)
                {
                    TraceOutput($"Attempt #{i + 1}");
                    measuredTime += RunProcessAndWaitForHealthCheck(@"dotnet", $@"""./{Path.GetFileNameWithoutExtension(projectPath)}.dll"" --urls {urlToTest}", tempDir, urlToTest);
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            TraceOutput($"Average time: {measuredTime / repeat}");

            TraceOutput($"Removing temp dir...");
            FileSystemHelper.RemoveDir(tempDir);
            TraceOutput($"Done");
        }

        private void RunProcessAndWait(string path, string arguments, string workingDirectory)
        {
            TraceOutput($"Running {path} {arguments}");

            var psi = new ProcessStartInfo(path, arguments) {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory
            };
            var process = Process.Start(psi);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception($"Process exited with code {process.ExitCode}!");
            }
        }

        private long RunProcessAndWaitForHealthCheck(string path, string arguments, string workingDirectory, string urlToTest)
        {
            var psi = new ProcessStartInfo(path, arguments) {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory
            };
            var process = Process.Start(psi);

            var sw = new Stopwatch();
            sw.Start();

            retry:
            try
            {
                var wc = new WebClient();
                var response = wc.DownloadString(urlToTest);
            }
            catch (WebException ex) when (ex.InnerException is HttpRequestException hrex && (hrex.InnerException is IOException || hrex.InnerException is SocketException))
            {
                Thread.Sleep(100);
                goto retry;
            }

            if (process.HasExited)
            {
                throw new Exception("The process has exited!");
            }

            var time = sw.ElapsedMilliseconds;
            ImportantOutput(time.ToString());

            process.Kill();
            return time;
        }

        public void ImportantOutput(string message)
        {
            Console.WriteLine(message);
        }

        public void TraceOutput(string message)
        {
            if (verbose)
            {
                Console.WriteLine(message);
            }
        }
    }
}
