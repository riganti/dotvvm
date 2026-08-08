using System.CommandLine;

namespace DotVVM.CommandLine
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var rootCmd = new RootCommand("DotVVM Command-Line Interface");
            rootCmd.AddInfoCommands();
            rootCmd.AddCompilerCommands();
            rootCmd.AddTemplateCommands();
            rootCmd.AddOpenApiCommands();
            rootCmd.AddVerboseOption();
            try
            {
                return rootCmd.Parse(args).Invoke();
            }
            finally
            {
                CommandLineExtensions.Factory.Dispose();
            }
        }
    }
}
