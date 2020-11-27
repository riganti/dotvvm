using System;
using System.Diagnostics;
using DotVVM.Utils.ProjectService.Output;

namespace DotVVM.Utils.ProjectService.Operations
{
    public class CommandRunner
    {
        public bool RunCommand(IOutputLogger logger, string command, params string[] arguments)
        {
            var args = string.Join(" ", arguments);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(command, args)
                {
                    CreateNoWindow = false,
                    UseShellExecute = false
                }
            };
            
            return RunProcess(logger, process);
        }

        public bool RunCommand(IOutputLogger logger, ProcessStartInfo startInfo)
        {
            var process = new Process
            {
                StartInfo = startInfo
            };
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = false;

            return RunProcess(logger, process);
        }

        private bool RunProcess(IOutputLogger logger, Process process)
        {
            logger.WriteInfo($"Command: {process.StartInfo.FileName} {process.StartInfo.Arguments}", ConsoleColor.Blue);

            process.Start();

            process.WaitForExit();
            return process.ExitCode == 0;
        }
    }
}