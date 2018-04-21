using System;
using System.IO;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.TypeScript.Compiler.Exceptions;
using DotVVM.TypeScript.Compiler.Utils;
using DotVVM.TypeScript.Compiler.Utils.IO;
using DotVVM.TypeScript.Compiler.Utils.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.TypeScript.Compiler
{
    class Program
    {

        public static CompilerArguments ParseArguments(string[] args)
        {
            if (args.Length != 2)
            {
                throw new MissingArgumentsException();
            }

            var filePath = args[0];
            var solutionFileInfo = new FileInfo(filePath);
            if(!solutionFileInfo.Exists)
                throw new InvalidArgumentException("Solution file does not exist.");
            if(!solutionFileInfo.Extension.Equals(".sln", StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidArgumentException("Passed file is not valid sln file.");

            var projectName = args[1];

            return new CompilerArguments {SolutionFile = solutionFileInfo, ProjectName = projectName};
        }

        static async Task<int> Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var fileStore = new LocalFileStore();
            try
            {
                var compilerArguments = ParseArguments(args);
                var compiler = new Compiler(compilerArguments, fileStore, logger);
                await compiler.RunAsync();
                return 0;
            }
            catch (InvalidArgumentException exception)
            {
                logger.LogError("Arguments", exception.Message);
            }
            catch (MissingArgumentsException)
            {
                logger.LogError("Arguments", "Some arguments are missing.");
                PrintHelp();
            }
            //catch (Exception exception)
            //{
            //    Console.WriteLine("An error occured during compilation: " + exception.Message);
            //}
            return 1;
        }

        private static void PrintHelp()
        {
            
        }
    }
}
