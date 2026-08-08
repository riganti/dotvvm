using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Frameworks;
using DotVVM.Framework.Utils;

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
                ArgumentList = { "-property",  "installationPath" },
                RedirectStandardOutput = true,
                FileName = vswhere.FullName,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var process = Process.Start(startInfo).NotNull();
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

            var buildProperties = properties?.ToArray();
            if (restore)
            {
                var restoreProperties = buildProperties?
                    .Where(p => !string.Equals(p.Key, "TargetFramework", StringComparison.OrdinalIgnoreCase));
                WriteStepHeading("Restoring packages", showOutput);
                if (!TryInvoke(GetProcessStartInfo(project, restoreProperties, new[] { "Restore" }, verbosity), showOutput, logger))
                {
                    return false;
                }
            }

            WriteStepHeading("Building project", showOutput);
            return TryInvoke(GetProcessStartInfo(project, buildProperties, targets ?? new[] { "Build" }, verbosity), showOutput, logger);
        }

        private static void WriteStepHeading(string step, bool showOutput)
        {
            if (!showOutput)
                return;

            Console.Out.WriteLine();
            Console.Out.WriteLine($"=== {step} ===");
            Console.Out.WriteLine();
        }

        private static bool TryInvoke(ProcessStartInfo startInfo, bool showOutput, ILogger logger)
        {
            if (!showOutput)
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
            }
            logger.LogDebug($"Invoking MSBuild with args: '{startInfo.ArgumentList.StringJoin(" ")}'.");
            var process = Process.Start(startInfo).NotNull();
            if (!showOutput)
            {
                Task.Run(() => process.StandardOutput.ReadToEnd());
                Task.Run(() => process.StandardError.ReadToEnd());
            }
            process.WaitForExit();
            return process.ExitCode == 0;
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
            string verbosity = "minimal")
        {
            var p = new ProcessStartInfo
            {
                FileName = ExecutablePath
            };
            foreach (var a in PrefixedArgs)
            {
                p.ArgumentList.Add(a);
            }
            p.ArgumentList.Add($"-verbosity:{verbosity}");
            if (properties is object)
            {
                foreach(var property in properties)
                {
                    p.ArgumentList.Add($"-property:{property.Key}={property.Value}");
                }
            }
            if (targets is object)
            {
                p.ArgumentList.Add($"-target:{string.Join(";", targets)}");
            }

            p.ArgumentList.Add($"{project.FullName}");

            return p;
        }
    }
}
