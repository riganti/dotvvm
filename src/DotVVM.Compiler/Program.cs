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

        public static void Run(FileInfo assembly, DirectoryInfo? projectDir)
        {
            var executor = (AppDomainCompilerExecutor)ProjectLoader.GetExecutor(assembly.FullName);
            executor.ExecuteCompile(assembly, projectDir, null);
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

            Run(new FileInfo(args[0]), new DirectoryInfo(args[1]));
            return 0;
        }
    }
}
