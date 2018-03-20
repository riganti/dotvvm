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
            if (args.Length != 1)
            {
                throw new MissingArgumentsException();
            }

            var filePath = args[0];
            var projectFileInfo = new FileInfo(filePath);
            if(!projectFileInfo.Exists)
                throw new InvalidArgumentException("Project file does not exist.");
            if(!projectFileInfo.Extension.Equals(".csproj", StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidArgumentException("Passed file is not valid csproj file.");

            return new CompilerArguments {ProjectFile = projectFileInfo};
        }

        static void Main(string[] args)
        {
            try
            {
                var compilerArguments = ParseArguments(args);
                var compiler = new Compiler(compilerArguments);
                compiler.RunAsync().GetAwaiter().GetResult();
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
        }

        private static void PrintHelp()
        {
            
        }
    }
}
