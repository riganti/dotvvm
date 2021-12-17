using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using Medallion.Shell;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Setup.Configuration;

namespace DotVVM.Tools.StartupPerfTester
{
    public class StartupPerformanceTest : IDisposable
    {
        public const string CommandName = "dotvvm-startup-perf";

        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger logger;
        private readonly FileInfo project;
        private readonly TestTarget type;
        private readonly int repeat;
        private readonly string url;
        private readonly int timeout;

        public StartupPerformanceTest(FileInfo project, TestTarget type, int repeat, string url, bool verbose, int timeout)
        {
            this.project = project;
            this.type = type;
            this.repeat = repeat;
            this.url = url;
            this.timeout = timeout;
            var logLevel = verbose ? LogLevel.Debug : LogLevel.Information;
            loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(logLevel));
            logger = loggerFactory.CreateLogger("dotvvm-startup-perf");

        }

        public int HandleCommand()
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

            var measuredTimes = new List<double>();
            if (type == TestTarget.Owin)
            {
                // OWIN
                var msbuildPath = FindVSMSBuild();
                if (msbuildPath is null)
                {
                    logger.LogCritical("A Visual Studio MSBuild could not be found.");
                    return 1;
                }
                TraceOutput($"Publishing...");
                var success = RunProcessAndWait(
                    path: msbuildPath,
                    arguments: new[]
                    {
                        Path.GetFileName(projectPath),
                        "/p:DeployOnBuild=true",
                        "/t:WebPublish",
                        "/p:WebPublishMethod=FileSystem",
                        $"/p:publishUrl={tempDir}"
                    },
                    workingDirectory: dir);
                if (!success)
                {
                    return 1;
                }

                for (var i = 0; i < repeat; i++)
                {
                    TraceOutput($"Attempt #{i + 1}");
                    measuredTimes.Add(RunProcessAndWaitForHealthCheck(@"C:\Program Files (x86)\IIS Express\iisexpress.exe",
                        new[] { $"/path:{dir}", $"/port:{port}" }, dir, urlToTest, timeout));
                }
            }
            else if (type == TestTarget.AspNetCore)
            {
                // ASP.NET Core
                TraceOutput($"Publishing...");
                if (!RunProcessAndWait(@"dotnet", new[] { "publish", "-c", "Release", "-o", tempDir }, dir))
                {
                    return 1;
                }

                for (var i = 0; i < repeat; i++)
                {
                    TraceOutput($"Attempt #{i + 1}");
                    measuredTimes.Add(RunProcessAndWaitForHealthCheck(@"dotnet",
                        new[] { $"./{Path.GetFileNameWithoutExtension(projectPath)}.dll", "--urls", urlToTest }, tempDir, urlToTest, timeout));
                }
            }
            else if (type == TestTarget.RunDotnet)
            {
                // Just launch a process
                for (var i = 0; i < repeat; i++)
                {
                    TraceOutput($"Attempt #{i + 1}");
                    measuredTimes.Add(RunProcessAndWaitForHealthCheck(@"dotnet", new[] { projectPath, "--urls", urlToTest }, ".", urlToTest, timeout));
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            ImportantOutput($"Average time: {measuredTimes.Average()}, Min time: {measuredTimes.Min()}, Max time: {measuredTimes.Max()}");

            TraceOutput($"Removing temp dir...");
            FileSystemHelper.RemoveTempDir(tempDir);
            TraceOutput($"Done");
            return 0;
        }

        private static Command RunProcess(string path, string[] arguments, string workingDirectory)
        {
            return Command.Run(path, arguments, options => options
                .WorkingDirectory(workingDirectory)
                .StartInfo(psi =>
                {
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                }));
        }

        private bool RunProcessAndWait(string path, string[] arguments, string workingDirectory)
        {
            TraceOutput($"Running {path} {string.Join(" ", arguments)}");
            var command = RunProcess(path, arguments, workingDirectory);
            command.Wait();

            if (!command.Result.Success)
            {
                TraceOutput(command.Result.StandardOutput);
                logger.LogCritical($"Process exited with code {command.Result.ExitCode}!");
                return false;
            }
            return true;
        }


        private double RunProcessAndWaitForHealthCheck(
            string path,
            string[] arguments,
            string workingDirectory,
            string urlToTest,
            int timeoutSeconds)
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
                Thread.Sleep(10);

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

            var time = sw.Elapsed.TotalMilliseconds;
            ImportantOutput(time.ToString());

            command.Kill();
            return time;
        }

        public void ImportantOutput(string message)
        {
            logger.LogInformation(message);
        }

        public void TraceOutput(string message)
        {
            logger.LogDebug(message);
        }

        private string FindVSMSBuild()
        {
            try
            {
                var query = new SetupConfiguration();
                var query2 = (ISetupConfiguration2)query;
                var @enum = query2.EnumAllInstances();
                var instances = new ISetupInstance[1];
                int fetchedCount;
                do
                {
                    @enum.Next(1, instances, out fetchedCount);
                    if (fetchedCount > 0)
                    {
                        var instance2 = (ISetupInstance2)instances[0];
                        var path = instance2.GetInstallationPath();
                        var exe = new FileInfo(Path.Combine(path, "MSBuild/Current/Bin/MSBuild.exe"));
                        if (exe.Exists)
                        {
                            return exe.FullName;
                        }
                    }
                }
                while (fetchedCount > 0);
                return null;
            }
            catch(PlatformNotSupportedException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            loggerFactory.Dispose();
        }
    }
}
