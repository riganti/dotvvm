using System.CommandLine;

namespace DotVVM.Tool
{
    public static class SeleniumGenerator
    {
        public static void AddSeleniumGenerator(Command command)
        {
            command.AddCommand(new Command("uitest", "Invokes the Selenium test generator."));
        }
    }
}
