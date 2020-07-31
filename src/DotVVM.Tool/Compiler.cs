using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace DotVVM.Tool
{
    public static class Compiler
    {
        public static void AddCompiler(Command command)
        {
            var compileCmd = new Command("compile", "Invoke the DotVVM compiler");
            compileCmd.AddArgument(new Argument<FileSystemInfo>("TARGET", "Path to a DotVVM project"));
            compileCmd.Handler = CommandHandler.Create(typeof(Compiler).GetMethod(nameof(ExecuteCommand))!);
            command.AddCommand(compileCmd);
        }

        public static void ExecuteCommand(FileSystemInfo target)
        {
            var msbuild = MSBuild.Create(target as FileInfo);
        }
    }
}
