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
            // get configuration
            var metadataService = new DotvvmProjectMetadataService();
            var metadata = metadataService.FindInDirectory(Directory.GetCurrentDirectory());
            if (metadata == null)
            {
                Console.WriteLine("No DotVVM project metadata was found on current path.");
                Console.WriteLine("Is the current directory the root directory of DotVVM project? [y] or [n]");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Console.WriteLine();
                    metadata = metadataService.CreateDefaultConfiguration(Directory.GetCurrentDirectory());
                    metadataService.Save(metadata);
                }
                else
                {
                    Console.WriteLine();
                    Environment.Exit(1);
                }
            }

            // find applicable command
            var commands = new CommandBase[]
            {
                new CreateProjectCommand(),

                new AddPageCommand(),
                new AddMasterPageCommand(),
                new AddViewModelCommand(),
                new AddControlCommand(),

                new GenerateUiTestStubCommand()
            };
            var arguments = new Arguments(args);
            var command = commands.FirstOrDefault(c => c.CanHandle(arguments, metadata));

            // execute the command
            if (command != null)
            {
                command.Handle(arguments, metadata);

                // save project metadata
                metadataService.Save(metadata);
            }
            else
            {
                throw new InvalidCommandUsageException("Invalid command!");
            }
        }
    }
}
