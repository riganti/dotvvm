using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine.Commands;
using DotVVM.CommandLine.Commands.Implementation;
using DotVVM.CommandLine.Metadata;

namespace DotVVM.CommandLine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var arguments = new Arguments(args);

            // get configuration
            var metadataService = new DotvvmProjectMetadataService();
            var metadata = metadataService.FindInDirectory(Directory.GetCurrentDirectory());
            if (metadata == null)
            {
                if (!arguments.HasOption("--silent"))
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
                else
                {
                    metadata = metadataService.CreateDefaultConfiguration(Directory.GetCurrentDirectory());
                    metadataService.Save(metadata);
                }
            }

            // find applicable command
            var commands = new CommandBase[]
            {
                new AddPageCommand(),
                new AddMasterPageCommand(),
                new AddViewModelCommand(),
                new AddControlCommand(),
                new AddNswagCommand(),
                new RegenNswagCommand()

                //new GenerateUiTestStubCommand()
            };
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
                    throw new InvalidCommandUsageException("Unknown command!");
                }
            }
            catch (InvalidCommandUsageException ex)
            {
                Console.WriteLine("Invalid Command Usage: " + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }
    }
}
