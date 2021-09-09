using System;
using System.IO;
using System.Linq;

namespace DotVVM.Compiler
{
    public static class Program
    {
        private static void PrintHelp(TextWriter? writer = null)
        {
            var executableName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            writer ??= Console.Error;
            writer.Write(
$@"Usage: {executableName} [OPTIONS] <ASSEMBLY> <PROJECT_DIR>

Arguments:
  <ASSEMBLY>     Path to a DotVVM project assembly.
  <PROJECT_DIR>  Path to a DotVVM project directory.

Options:
  -h|-?|--help   Print this help text.
  --list-props   Print a list of DotVVM properties inside the assembly.
");
        }

        public static int Main(string[] args)
        {
            if (!CompilerArgs.TryParse(args, out var parsed))
            {
                PrintHelp(Console.Error);
                return 1;
            }

            if (parsed.IsHelp)
            {
                PrintHelp(Console.Out);
                return 0;
            }

            var executor = ProjectLoader.GetExecutor(parsed.AssemblyFile.FullName);
            var success = executor.ExecuteCompile(parsed);
            return success ? 0 : 1;
        }
    }
}
