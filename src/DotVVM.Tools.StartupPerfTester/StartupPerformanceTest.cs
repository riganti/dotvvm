using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using Medallion.Shell;

namespace DotVVM.Tools.StartupPerfTester
{
    public class StartupPerformanceTest
    {
        private readonly FileInfo project;
        private readonly TestTarget type;
        private readonly int repeat;
        private readonly string url;
        private readonly bool verbose;
        private readonly int timeout;

        public StartupPerformanceTest(FileInfo project, TestTarget type, int repeat, string url, bool verbose, int timeout)
        {
            this.project = project;
            this.type = type;
            this.repeat = repeat;
            this.url = url;
            this.verbose = verbose;
            this.timeout = timeout;
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
                RunProcessAndWait(@"c:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
                    new[] { Path.GetFileName(projectPath), "/p:DeployOnBuild=true", "/t:WebPublish", "/p:WebPublishMethod=FileSystem", $"/p:publishUrl={tempDir}" }, dir);

                for (var i = 0; i < repeat; i++)
                {
                    TraceOutput($"Attempt #{i + 1}");
                    measuredTime += RunProcessAndWaitForHealthCheck(@"C:\Program Files (x86)\IIS Express\iisexpress.exe",
                        new[] { $"/path:{dir}", $"/port:{port}" }, dir, urlToTest, timeout);
                }
            }
            else if (type == TestTarget.AspNetCore)
            {
                // ASP.NET Core
                TraceOutput($"Publishing...");
                RunProcessAndWait(@"dotnet", new[] { "publish", "-c", "Release", "-o", tempDir }, dir);

                for (var i = 0; i < repeat; i++)
                {
                    TraceOutput($"Attempt #{i + 1}");
                    measuredTime += RunProcessAndWaitForHealthCheck(@"dotnet",
                        new[] { $"./{Path.GetFileNameWithoutExtension(projectPath)}.dll", "--urls", urlToTest }, tempDir, urlToTest, timeout);
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            TraceOutput($"Average time: {measuredTime / repeat}");

            TraceOutput($"Removing temp dir...");
            FileSystemHelper.RemoveTempDir(tempDir);
            TraceOutput($"Done");
        }
        private static Command RunProcess(string path, string[] arguments, string workingDirectory)
        {
            return Command.Run(path, arguments, options => options
                .WorkingDirectory(workingDirectory)
                .StartInfo(psi => {
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                }));
        }

        private void RunProcessAndWait(string path, string[] arguments, string workingDirectory)
        {
            TraceOutput($"Running {path} {string.Join(" ", arguments)}");
            var command = RunProcess(path, arguments, workingDirectory);
            command.Wait();

            if (!command.Result.Success)
            {
                TraceOutput(command.Result.StandardOutput);
                throw new Exception($"Process exited with code {command.Result.ExitCode}!");
            }
        }

        
        private long RunProcessAndWaitForHealthCheck(string path, string[] arguments, string workingDirectory, string urlToTest, int timeoutSeconds)
        {
            var command = RunProcess(path, arguments, workingDirectory);

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

                if (command.Task.IsCompleted)
                {
                    TraceOutput(command.Result.StandardOutput);
                    throw new Exception("The process has exited!");
                }

                if (sw.Elapsed.TotalSeconds > timeoutSeconds)
                {
                    command.Kill();
                    TraceOutput(command.Result.StandardOutput);
                    throw new Exception($"The process didn't open the HTTP port withing the timeout of {timeoutSeconds} secs. If this is expected, use --timeout option to increase the timeout.");
                }

                goto retry;
            }

            var time = sw.ElapsedMilliseconds;
            ImportantOutput(time.ToString());

            command.Kill();
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
