using System.CommandLine;
using System.CommandLine.Builder;

namespace DotVVM.Tool
{
    public static class Program
    {
        public const string MSBuildOutputAlias = "--msbuild-output";

        public static int Main(string[] args)
        {
            var rootCmd = new RootCommand("DotVVM Command-Line Interface")
            {
                Name = "dotvvm"
            };
            rootCmd.AddCompilerCommands();
            rootCmd.AddSeleniumGeneratorCommands();
            rootCmd.AddTemplateCommands();
            rootCmd.AddOpenApiCommands();
            rootCmd.AddVerboseOption();
            rootCmd.AddGlobalOption(new Option<bool>(
                alias: MSBuildOutputAlias,
                description: "Show output from MSBuild invocations"));
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
