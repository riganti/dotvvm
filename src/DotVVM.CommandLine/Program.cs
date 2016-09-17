using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.CommandLine.Commands;
using DotVVM.CommandLine.Commands.Implementation;

namespace DotVVM.CommandLine
{
    public class Program
    {
        public static void Main(string[] args)
        {
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

            var command = commands.FirstOrDefault(c => c.CanHandle(arguments));
            if (command != null)
            {
                command.Handle(arguments);
            }
            else
            {
                throw new InvalidCommandUsageException("Invalid command!");
            }
        }
    }
}
