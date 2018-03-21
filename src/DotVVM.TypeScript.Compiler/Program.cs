using System;
using System.IO;
using System.Threading.Tasks;
using DotVVM.TypeScript.Compiler.Exceptions;

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

        static int Main(string[] args)
        {
            try
            {
                var compilerArguments = ParseArguments(args);
                var compiler = new Compiler(compilerArguments);
                compiler.RunAsync().GetAwaiter().GetResult();
                return 0;
            }
            catch (InvalidArgumentException exception)
            {
                Console.WriteLine(exception.Message);
            }
            catch (MissingArgumentsException)
            {
                Console.WriteLine("Some arguments are missing.");
                PrintHelp();
            }
            catch (Exception exception)
            {
                Console.WriteLine("An error occured during compilation: " + exception.Message);
            }
            return 1;
        }

        private static void PrintHelp()
        {
            
        }
    }
}
