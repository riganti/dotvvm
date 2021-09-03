using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Frameworks;

namespace DotVVM.CommandLine
{
    public class MSBuild
    {
        public const string VSRelativePath = "MSBuild/Current/Bin/MSBuild.exe";

        public string ExecutablePath { get; } = string.Empty;
        public ImmutableArray<string> PrefixedArgs { get; } = ImmutableArray.Create<string>();

        public MSBuild(string executablePath, ImmutableArray<string> prefixedArgs)
        {
            ExecutablePath = executablePath;
            PrefixedArgs = prefixedArgs;
        }

        public static MSBuild CreateFromSdk()
        {
            return new MSBuild(
                executablePath: "dotnet",
                prefixedArgs: ImmutableArray.Create("msbuild", "/nologo"));
        }

        public static MSBuild? CreateFromVS()
        {
            var dir = Path.GetDirectoryName(typeof(MSBuild).Assembly.Location)!;
            var vswhere = new FileInfo(Path.Combine(dir, "vswhere.exe"));
            if (!vswhere.Exists)
            {
                throw new InvalidOperationException($"To use '{nameof(CreateFromVS)}' you must include vswhere.exe.");
            }

            var startInfo = new ProcessStartInfo
            {
                Arguments = "-property installationPath",
                RedirectStandardOutput = true,
                FileName = vswhere.FullName,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var process = Process.Start(startInfo);
            var stdout = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                return null;
            }

            var msbuildExe = new FileInfo(Path.Combine(stdout, VSRelativePath));
            if (!msbuildExe.Exists)
            {
                return null;
            }

            return new MSBuild(msbuildExe.FullName, ImmutableArray.Create("/nologo"));
        }

        public static MSBuild? CreateForNuGetFramework(NuGetFramework? target)
        {
            var msbuildVs = CreateFromVS();
            var msbuildSdk = CreateFromSdk();

            if (target is null || target.IsDesktop())
            {
                // prefer VS's MSBuild for .NET Framework
                return msbuildVs ?? msbuildSdk;
            }
            return msbuildSdk;
        }

        public bool TryBuild(
            FileInfo project,
            string configuration,
            string targetFramework,
            bool showOutput = false,
            ILogger? logger = null)
        {
            return TryInvoke(
                project: project,
                properties: new Dictionary<string, string>
                {
                    ["Configuration"] = configuration,
                    ["TargetFramework"] = targetFramework
                },
                restore: true,
                showOutput: showOutput,
                logger: logger);
        }

        public bool TryInvoke(
            FileInfo project,
            IEnumerable<KeyValuePair<string, string>>? properties = null,
            IEnumerable<string>? targets = null,
            bool restore = false,
            string verbosity = "minimal",
            bool showOutput = false,
            ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

            var startInfo = GetProcessStartInfo(project, properties, targets, restore, verbosity);
            if (!showOutput)
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
            }
            logger.LogDebug($"Invoking MSBuild with args: '{startInfo.Arguments}'.");
            var process = Process.Start(startInfo);
            if (!showOutput)
            {
                Task.Run(() => process.StandardOutput.ReadToEnd());
                Task.Run(() => process.StandardError.ReadToEnd());
            }
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        public (bool, string, string) TryInvokeWithOutput(
            FileInfo project,
            IEnumerable<KeyValuePair<string, string>>? properties = null,
            IEnumerable<string>? targets = null,
            bool restore = false,
            string verbosity = "minimal",
            ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

            var startInfo = GetProcessStartInfo(project, properties, targets, restore, verbosity);
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            logger.LogDebug($"Invoking MSBuild with args: '{startInfo.Arguments}'.");
        
            var process = Process.Start(startInfo);
            var stderrTask = Task.Run(() => process.StandardError.ReadToEnd());
            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = stderrTask.GetAwaiter().GetResult();
            process.WaitForExit();
            return (process.ExitCode == 0, stdout, stderr);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('[');
            sb.Append(ExecutablePath);
            if (PrefixedArgs.Length > 0)
            {
                sb.Append(' ');
                sb.Append(string.Join(" ", PrefixedArgs));
            }
            sb.Append(']');
            return sb.ToString();
        }

        private ProcessStartInfo GetProcessStartInfo(
            FileInfo project,
            IEnumerable<KeyValuePair<string, string>>? properties = null,
            IEnumerable<string>? targets = null,
            bool restore = false,
            string verbosity = "minimal")
        {
            var sb = new StringBuilder();
            sb.Append(string.Join(" ", PrefixedArgs));
            if (restore)
            {
                sb.Append(" -restore");
            }
            sb.Append($" -verbosity:{verbosity}");
            if (properties is object)
            {
                foreach(var property in properties)
                {
                    sb.Append($" -property:{property.Key}={property.Value}");
                }
            }
            if (targets is object)
            {
                sb.Append($" -target:{string.Join(";", targets)}");
            }

            sb.Append($" {project.FullName}");
            return new ProcessStartInfo
            {
                FileName = ExecutablePath,
                Arguments = sb.ToString()
            };
        }
    }
}
