using System.CommandLine;

namespace DotVVM.Tool
{
    public static class SeleniumGeneratorCommands
    {
        public static void AddSeleniumGeneratorCommands(this Command command)
        {
            command.AddCommand(new Command("uitest", "Invokes the Selenium test generator."));
        }
    }
}
