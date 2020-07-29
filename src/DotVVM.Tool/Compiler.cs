using System.CommandLine;

namespace DotVVM.Tool
{
    public static class Compiler
    {
        public static void AddCompiler(Command command)
        {
            command.AddCommand(new Command("compiler", "Invoke the DotVVM compiler"));
        }
    }
}
