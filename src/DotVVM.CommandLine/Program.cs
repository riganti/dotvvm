    using System;
using System.IO;
using System.Linq;
using DotVVM.CommandLine.Commands.Core;
using DotVVM.CommandLine.Commands.Handlers;
using DotVVM.CommandLine.Core;
    using DotVVM.CommandLine.Core.Arguments;
    using DotVVM.CommandLine.Core.Metadata;

namespace DotVVM.CommandLine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // get configuration
            var metadataService = new DotvvmProjectMetadataService();
            var metadata = metadataService.FindInDirectory(Directory.GetCurrentDirectory());
            if (metadata == null)
            {
                Console.WriteLine("No DotVVM project metadata file (.dotvvm.json) was found on current path.");
                if (ConsoleHelpers.AskForYesNo("Is the current directory the root directory of DotVVM project?"))
                {
                    Console.WriteLine();
                    metadata = metadataService.CreateDefaultConfiguration(Directory.GetCurrentDirectory());
                    metadataService.Save(metadata);
                }
                else
                {
                    Console.WriteLine("There is no DotVVM project metadata file!");
                    Environment.Exit(1);
                }
            }

            // find applicable command
            var commands = new CommandBase[]
            {
                new AddPageCommandHandler(),
                new AddMasterPageCommandHandler(),
                new AddViewModelCommandHandler(),
                new AddControlCommandHandler(),
                new AddNswagCommandHandler(),
                new RegenNswagCommandHandler(),
                new CompilerConfigurationExportCommandHandler(),
                new VersionCommandHandler(), 
                new GenerateUiTestStubCommandHandler()
            };
            var arguments = new Arguments(args);
            var command = commands.FirstOrDefault(c => c.TryConsumeArgs(arguments, metadata));

            // execute the command
            try
            {
                if (command != null)
                {
                    command.Handle(arguments, metadata);

                    // save project metadata
                    metadataService.Save(metadata);
                }
                else
                {
                    WriteHelp(commands);

                }
            }
            catch (InvalidCommandUsageException ex)
            {
                Console.WriteLine("Invalid Command Usage: " + ex);
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }

        private static void WriteHelp(CommandBase[] commands )
        {
            var indent = "    ";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Command could not be found.");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("DotVVM CLI");
            foreach (var command in commands)
            {
                Console.WriteLine();
                Console.WriteLine(indent + "- " + command.Name);
                foreach (var usage in command.Usages) Console.WriteLine(indent + indent + usage);
            }
            Console.WriteLine();
            Console.WriteLine();
            Environment.Exit(1);
        }
    }
}
