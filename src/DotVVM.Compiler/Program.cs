using System;
using System.IO;
using System.Linq;

namespace DotVVM.Compiler
{
    public static class Program
    {
        private static readonly string[] HelpOptions = new string[] {
            "--help", "-h", "-?", "/help", "/h", "/?"
        };

        public static bool TryRun(FileInfo assembly, DirectoryInfo? projectDir)
        {
            var executor = ProjectLoader.GetExecutor(assembly.FullName);
            return executor.ExecuteCompile(assembly, projectDir, null);
        }

        public static int Main(string[] args)
        {
            // To minimize dependencies, this tool deliberately reinvents the wheel instead of using System.CommandLine.
            if (args.Length != 2 || (args.Length == 1 && HelpOptions.Contains(args[0])))
            {
                Console.Error.Write(
@"Usage: DotVVM.Compiler <ASSEMBLY> <PROJECT_DIR>

Arguments:
  <ASSEMBLY>     Path to a DotVVM project assembly.
  <PROJECT_DIR>  Path to a DotVVM project directory.");
                return 1;
            }

            var assemblyFile = new FileInfo(args[0]);
            if (!assemblyFile.Exists)
            {
                Console.Error.Write($"Assembly '{assemblyFile}' does not exist.");
                return 1;
            }

            var projectDir = new DirectoryInfo(args[1]);
            if (!projectDir.Exists)
            {
                Console.Error.Write($"Project directory '{projectDir}' does not exist.");
                return 1;
            }

            var success = TryRun(assemblyFile, projectDir);
            return success ? 0 : 1;
        }
    }
}
