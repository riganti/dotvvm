using System;
using System.Diagnostics;
using DotVVM.CommandLine.Commands.Core;
using DotVVM.CommandLine.Core.Metadata;
using DotVVM.Compiler;
using Newtonsoft.Json;

namespace DotVVM.CommandLine.Commands.Logic.SeleniumGenerator
{
    public class SeleniumGeneratorLauncher
    {
        public static void Start(Arguments args, DotvvmProjectMetadata dotvvmProjectMetadata)
        {
            var metadata = JsonConvert.SerializeObject(JsonConvert.SerializeObject(dotvvmProjectMetadata));
            var exited = false;

            var processArgs = $"{CompilerConstants.Arguments.JsonOptions} {metadata} ";
            var i = 0;
            while (args[i] != null)
            {
                processArgs += $"{args[i]} ";
                i++;
            }

            var processInfo = new ProcessStartInfo(@"C:\Users\filipkalous\source\repos\dotvvm-selenium-generator.git\src\DotVVM.Framework.Tools.SeleniumGenerator\bin\Debug\netcoreapp2.0\DotVVM.Framework.Tools.SeleniumGenerator.exe")
            {
                RedirectStandardError = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false,
                Arguments = processArgs
            };

            var process = new Process
            {
                StartInfo = processInfo
            };

            process.OutputDataReceived += (sender, eventArgs) =>
            {
                if (eventArgs?.Data?.StartsWith("#$") ?? false)
                {
                    exited = true;
                }

                Console.WriteLine(eventArgs?.Data);
            };

            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                if (eventArgs?.Data?.StartsWith("#$") ?? false)
                {
                    exited = true;
                }

                Console.WriteLine(eventArgs?.Data);
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
