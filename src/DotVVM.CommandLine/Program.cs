using System.CommandLine;
using System.CommandLine.Builder;

namespace DotVVM.CommandLine
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var rootCmd = new RootCommand("DotVVM Command-Line Interface")
            {
                Name = "dotvvm"
            };
            rootCmd.AddInfoCommands();
            rootCmd.AddCompilerCommands();
            rootCmd.AddTemplateCommands();
            rootCmd.AddOpenApiCommands();
            rootCmd.AddVerboseOption();
            // awkwardly enough, the built parser attaches itself to the command by itself
            new CommandLineBuilder(rootCmd)
                .UseDefaults()
                .UseLogging()
                .UseDotvvmMetadata()
                .Build();
            return rootCmd.Invoke(args);
        }
    }
}
