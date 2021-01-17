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
            if (args.Length != 2 || (args.Length == 1 && HelpOptions.Contains(args[0])))
            {
                Console.Error.Write(
@"Usage: DotVVM.Compiler <ASSEMBLY> <PROJECT_DIR>

Arguments:
  <ASSEMBLY>     Path to a DotVVM project assembly.
  <PROJECT_DIR>  Path to a DotVVM project directory.");
                return 1;
            }

            var success = TryRun(new FileInfo(args[0]), new DirectoryInfo(args[1]));
            return success ? 0 : 1;
        }
    }
}
