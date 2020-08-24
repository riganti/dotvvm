using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;

namespace DotVVM.CommandLine
{
    public static class InfoCommands
    {
        public static void AddInfoCommands(this Command command)
        {
            var infoCmd = new Command("info", "Prints metadata about the DotVVM project")
            {
                Handler = CommandHandler.Create(typeof(InfoCommands).GetMethod(nameof(HandleInfo))!)
            };
            infoCmd.AddTargetArgument();
            command.AddCommand(infoCmd);
        }

        public static void HandleInfo(
            ProjectMetadata metadata,
            ILogger logger)
        {
            logger.LogInformation($"DotVVM Project at '{metadata.ProjectDirectory}'");
        }
    }
}
