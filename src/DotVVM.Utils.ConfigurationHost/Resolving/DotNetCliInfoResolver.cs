using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DotVVM.Compiler.DTOs;

namespace DotVVM.Compiler.Resolving
{
    public class DotNetCliInfoResolver
    {
        public static DotNetCliInfo GetInfo()
        {
            var errors = new List<string>();
            var output = new List<string>();
            var startInfo = new ProcessStartInfo("dotnet", "--info");
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.ErrorDialog = false;
            startInfo.UseShellExecute = false;

            var dotnetProcess = new Process {
                StartInfo = startInfo
            };

            dotnetProcess.ErrorDataReceived += (sender, args) => {
                if (args.Data != null && !string.IsNullOrWhiteSpace(args.Data))
                {
                    errors.Add(args.Data);
                }
            };
            dotnetProcess.OutputDataReceived += (sender, args) => {
                output.Add(args.Data);
            };


            dotnetProcess.Start();
            dotnetProcess.BeginOutputReadLine();
            dotnetProcess.BeginErrorReadLine();

            dotnetProcess.WaitForExit(1000);

            if (errors.Any() || !output.Any())
            {
                return null;
            }

            return ParseInfo(output);

        }

        private static DotNetCliInfo ParseInfo(List<string> output)
        {
            var info = new DotNetCliInfo();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            foreach (var line in output)
            {
                ParseLine(line.Trim(), "OS Platform", value => { info.OSPlatform = value; });
                ParseLine(line.Trim(), "OS Name", value => { info.OSName = value; });
                ParseLine(line.Trim(), "RID", value => { info.RuntimeIdentifier = value; });
                ParseLine(line.Trim(), "Base Path", value => { info.BasePath = value; });
                ParseLine(line.Trim(), "Version", value => { info.Version = value; });
                ParseLine(line.Trim(), "Build", value => { info.Build = value; });
            }

            ComposeStorePath(info);


            return info;
        }

        private static void ComposeStorePath(DotNetCliInfo info)
        {
            var dir = new DirectoryInfo(info.BasePath);
            var tmpDir = dir;
            string dotnetRootPath = null;

            while (tmpDir.Parent != null && tmpDir.Parent.FullName != tmpDir.FullName)
            {
                if (tmpDir.Name.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
                {
                    dotnetRootPath = tmpDir.FullName;
                    break;
                }
                tmpDir = tmpDir.Parent;
            }

            var x64 = Path.Combine(dotnetRootPath, "store\\x64");
            var x86 = Path.Combine(dotnetRootPath, "store\\x64");
            if (Directory.Exists(x64))
            {
                info.Store = x64;
            }
            else if (Directory.Exists(x86))
            {
                info.Store = x86;
            }
        }

        private static void ParseLine(string line, string key, Action<string> parse)
        {
            int index;
            if ((index = line.IndexOf(":", StringComparison.Ordinal)) > 0)
            {
                var part1 = line.Substring(0, index);
                var part2 = line.Substring(index + 1, line.Length - index - 1);
                if (part1.Contains(key))
                {
                    parse(part2.Trim());
                }
            }
        }
    }
}
