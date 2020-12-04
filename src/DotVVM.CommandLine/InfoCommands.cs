using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
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
            DotvvmProject project,
            ILogger logger)
        {
            logger.LogInformation(
$@"Project metadata of '{project.ProjectFilePath}':
    AssemblyName: {project.AssemblyName}
    RootNamespace: {project.RootNamespace}
    PackageVersion: {project.PackageVersion}
    TargetFrameworks: {string.Join(", ", project.TargetFrameworks.Select(s => s.GetShortFolderName()))}");        }
    }
}
