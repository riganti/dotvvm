using System;
using System.Diagnostics;
using DotVVM.Compiler;
using Newtonsoft.Json;

namespace DotVVM.CommandLine.Commands.Logic.Compiler
{
    public class DotvvmCompilerLauncher
    {
        public static void Start(CompilerStartupOptions options)
        {
            // serialize object and encode the string for command line
            var opt = JsonConvert.SerializeObject(JsonConvert.SerializeObject(options.Options));
            bool exited = false;

            var processArgs = $"{(options.WaitForDebugger ? CompilerConstants.Arguments.WaitForDebugger : "")} {(options.WaitForDebugger ? CompilerConstants.Arguments.WaitForDebuggerAndBreak : "")} {CompilerConstants.Arguments.JsonOptions} {opt}";
            var processInfo =
                new ProcessStartInfo(options.CompilerExePath) {
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
        }
    }
}
