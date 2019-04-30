using System;
using System.Diagnostics;
using DotVVM.Compiler;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Operations.Providers;
using Newtonsoft.Json;

namespace DotVVM.CommandLine.Commands.Logic.Compiler
{
    public class DotvvmCompilerLauncher
    {
        public static void Start(CompilerStartupOptions options, IResolvedProjectMetadata metadata)
        {
            var compilerMeta = new DotvvmCompilerProvider().GetPreparedTool(metadata);
            if (compilerMeta == null) throw new Exception("Could not find DotVVM Compiler executable.");

            // serialize object and encode the string for command line
            var opt = JsonConvert.SerializeObject(JsonConvert.SerializeObject(options.Options));
            var exited = false;

            var processArgs = $"{(options.WaitForDebugger ? CompilerConstants.Arguments.WaitForDebugger : "")} {(options.WaitForDebugger ? CompilerConstants.Arguments.WaitForDebuggerAndBreak : "")} {CompilerConstants.Arguments.JsonOptions} {opt}";
            var executable = compilerMeta.MainModulePath;
            if (compilerMeta.Version == DotvvmToolExecutableVersion.DotNetCore)
            {
                executable = $"dotnet";
                processArgs = $"{JsonConvert.SerializeObject(compilerMeta.MainModulePath)} {processArgs}";
            }

            var processInfo = new ProcessStartInfo(executable) {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = processArgs,
                Environment = {
                    [CompilerConstants.EnvironmentVariables.AssemblySearchPath] = options.Options.WebSiteAssembly,
                    [CompilerConstants.EnvironmentVariables.WebAssemblyPath] = options.Options.WebSitePath,
                }
            };

            var process = new Process {
                StartInfo = processInfo
            };
            process.OutputDataReceived += (sender, eventArgs) => {
                if (eventArgs?.Data?.StartsWith("#$") ?? false)
                {
                    exited = true;
                }

                Console.WriteLine(eventArgs.Data);
            };
            process.ErrorDataReceived += (sender, eventArgs) => {
                if (eventArgs?.Data?.StartsWith("#$") ?? false)
                {
                    exited = true;
                }

                Console.WriteLine(eventArgs.Data);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            while (!exited)
            {
            }
            DotvvmToolProvider.Clean(compilerMeta);
        }
    }
}
