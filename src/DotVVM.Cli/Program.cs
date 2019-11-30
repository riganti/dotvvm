using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace DotVVM.Cli
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand();
            var exportConfig = new Command("export-config");
            {
                var targetArgument = new Argument("target") {
                    Arity = ArgumentArity.ZeroOrOne
                };
                targetArgument.SetDefaultValue(Directory.GetCurrentDirectory());
                exportConfig.Add(targetArgument);
                exportConfig.Handler = CommandHandler.Create<string>(ExportConfiguration.Invoke);
            }
            rootCommand.Add(exportConfig);
            return rootCommand.Invoke(args);
        }
    }
}
