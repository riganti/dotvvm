using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualBasic;
using Process = System.Diagnostics.Process;

namespace DotVVM.Framework.StartupPerfTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<TestTarget>(
                    new [] { "-t", "--target" },
                    "Target version - use 'owin' or 'aspnetcore'"
                ),
                new Option<int>(
                    new [] { "-r", "--repeat" },
                    () => 1,
                    "How many times the operation should be repeated."
                ),
                new Option<bool>(
                    new [] { "-v", "--verbose" },
                    () => false,
                    "Diagnostics output"
                )
            };
            rootCommand.Handler = CommandHandler.Create<TestTarget, int, bool>((target, repeat, verbose) =>
            {
                void WriteVerboseOutput(string message)
                {
                    if (verbose)
                    {
                        Console.WriteLine(message);
                    }
                }

                // test port availability
                var port = 64000;
                var urlToTest = $"http://localhost:{port}/";
                if (!TestPort(port))
                {
                    throw new Exception("The port 64000 is occupied!");
                }

                // prepare temp directory
                var dir = typeof(Program).Assembly.Location;
                dir = dir.Substring(0, dir.LastIndexOf("src", StringComparison.CurrentCultureIgnoreCase) + 3);
                WriteVerboseOutput($"Solution dir: {dir}");

                var tempDir = CreateTempDir();
                WriteVerboseOutput($"Temp dir: {tempDir}");

                WriteVerboseOutput($"Creating temp content dir...");
                var tempContentDir = CreateTempContentDir();
                CopyFiles(dir, tempContentDir, "DotVVM.Samples.Common");

                long measuredTime = 0;
                if (target == TestTarget.Owin)
                {
                    // OWIN
                    dir = Path.Combine(dir, "DotVVM.Samples.BasicSamples.Owin");

                    WriteVerboseOutput($"Publishing...");
                    RunProcessAndWait(@"c:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe", @$"DotVVM.Samples.BasicSamples.Owin.csproj /p:DeployOnBuild=true /t:WebPublish /p:WebPublishMethod=FileSystem /p:publishUrl=""{tempDir}""", dir);

                    for (var i = 0; i < repeat; i++)
                    {
                        WriteVerboseOutput($"Attempt #{i + 1}");
                        measuredTime += RunProcessAndWaitForHealthCheck(@"C:\Program Files (x86)\IIS Express\iisexpress.exe", $@"""/path:{dir}"" /port:{port}", dir, urlToTest);
                    }
                }
                else if (target == TestTarget.AspNetCore)
                {
                    // ASP.NET Core
                    dir = Path.Combine(dir, "DotVVM.Samples.BasicSamples.AspNetCoreLatest");

                    WriteVerboseOutput($"Publishing...");
                    RunProcessAndWait(@"dotnet", $"publish -c Release -o {tempDir}", dir);

                    for (var i = 0; i < repeat; i++)
                    {
                        WriteVerboseOutput($"Attempt #{i + 1}");
                        measuredTime += RunProcessAndWaitForHealthCheck(@"dotnet", $@"./DotVVM.Samples.BasicSamples.AspNetCoreLatest.dll --urls http://localhost:{port}/", tempDir, urlToTest);
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }

                WriteVerboseOutput($"Average time: {measuredTime / repeat}");

                WriteVerboseOutput($"Removing temp dir...");
                RemoveDir(tempDir);
                RemoveDir(tempContentDir);
                WriteVerboseOutput($"Done");
            });

            rootCommand.Invoke(args);
        }

        private static bool TestPort(int port)
        {
            using (var client = new TcpClient())
            {
                try
                {
                    client.Connect(new IPEndPoint(IPAddress.Loopback, port));
                    client.Close();
                    return false;
                }
                catch (Exception)
                {
                    return true;
                }
            }
        }

        private static string CreateTempContentDir()
        {
            var tempContentDir = Path.Combine(Path.GetTempPath(), "DotVVM.Samples.Common");
            if (!RemoveDir(tempContentDir))
            {
                throw new Exception("Cannot delete temp content directory!");
            }
            return tempContentDir;
        }


        private static void CopyFiles(string srcDir, string targetDir, string name)
        {
            RunProcessAndWait("xcopy", @$"/e /y /i ""{Path.Combine(srcDir, name)}"" ""{targetDir}""", srcDir);
        }

        private static string CreateTempDir()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        private static bool RemoveDir(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void RunProcessAndWait(string path, string arguments, string workingDirectory)
        {
            var psi = new ProcessStartInfo(path, arguments) {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory
            };
            var process = Process.Start(psi);
            process.WaitForExit();
        }

        private static long RunProcessAndWaitForHealthCheck(string path, string arguments, string workingDirectory, string urlToTest)
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
            Console.WriteLine(time);

            process.Kill();
            return time;
        }
    }

    public enum TestTarget
    {
        Owin,
        AspNetCore
    }
}
