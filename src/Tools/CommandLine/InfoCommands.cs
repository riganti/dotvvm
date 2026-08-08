using System.CommandLine;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DotVVM.CommandLine
{
    public static class InfoCommands
    {
        public static void AddInfoCommands(this Command command)
        {
            var infoCmd = new Command("info", "Prints metadata about the DotVVM project");
            infoCmd.AddTargetArgument();
            infoCmd.SetAction(parseResult =>
            {
                if (!CommandLineExtensions.TryGetProject(parseResult, out var project, out var logger))
                    return 1;

                HandleInfo(project, logger);
                return 0;
            });
            command.Subcommands.Add(infoCmd);
        }

        public static void HandleInfo(
            DotvvmProject project,
            ILogger logger)
        {
            logger.LogInformation(
$@"Project metadata of '{project.ProjectFilePath}':
    AssemblyName: {project.AssemblyName}
    OutputPath: {project.OutputPath}
    RootNamespace: {project.RootNamespace}
    PackageVersion: {project.PackageVersion}
    TargetFrameworks: {string.Join(", ", project.TargetFrameworks.Select(s => s.GetShortFolderName()))}");        }
    }
}
