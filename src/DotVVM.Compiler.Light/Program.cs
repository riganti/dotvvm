using System;
using System.Linq;
using System.Runtime.Loader;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;

namespace DotVVM.Compiler.Light
{
    public static class Program
    {
        private static Stopwatch stopwatcher;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                while (true)
                {
                    var optionsJson = ReadJsonFromStdin();
                    DoCompile(GetCompilerOptions(optionsJson));
                }
            }

            if (args[0] == "--debugger")
            {
                WaitForDbg();
                args = args.Skip(1).ToArray();
            }

            if (args[0] == "--json")
            {
                var opt = string.Join(" ", args.Skip(1));

                if (!DoCompile(GetCompilerOptions(opt)))
                {
                    Environment.Exit(1);
                }
            }
            else
            {
                var file = string.Join(" ", args);
                if (!DoCompileFromFile(file))
                {
                    Environment.Exit(1);
                }
            }
        }
        
        private static void WaitForDbg()
        {
            while (!Debugger.IsAttached) Thread.Sleep(10);
        }
        
        private static string ReadJsonFromStdin()
        {
            var optionsJson = ReadFromStdin();
            if (string.IsNullOrWhiteSpace(optionsJson))
            {
                Environment.Exit(0);
            }
            return optionsJson;
        }

        private static bool DoCompileFromFile(string file)
        {
            var optionsJson = File.ReadAllText(file);
            var compilerOptions = GetCompilerOptions(optionsJson);
            return DoCompile(compilerOptions);
        }

        private static bool DoCompile(CompilerOptions options)
        {
            InitStopwacher();
            WriteInfo("Starting compilation...");

            //Dont touch anything until the paths are filled we have to touch Json though
            try
            {
                CompilationResult result;
                if (options.FullCompile)
                {
                    // compile views
                    throw new NotSupportedException("Full compilation on .NET Core is not supported yet!");
                }
                else
                {
                    // only export configuration
                    result = ExportConfiguration(options);
                }
                Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }));

                Console.WriteLine();
                return true;
            }
            catch (Exception ex)
            {
                WriteError(ex);
                return false;
            }
        }
        
        private static CompilationResult ExportConfiguration(CompilerOptions options)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(options.WebSiteAssembly);
            var config = AspNetCoreInitializer.InitDotVVM(assembly, options.WebSitePath, (s) => { });
            return new CompilationResult()
            {
                Configuration = config
            };
        }

        private static void InitStopwacher()
        {
            stopwatcher = Stopwatch.StartNew();
        }

        private static CompilerOptions GetCompilerOptions(string optionsJson)
        {
            var options = new CompilerOptions();
            try
            {
                options = JsonConvert.DeserializeObject<CompilerOptions>(optionsJson);
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }

            return options;
        }

        private static void WriteError(Exception ex)
        {
            WriteInfo("Error occured!");
            var exceptionJson = JsonConvert.SerializeObject(ex);
            Console.WriteLine("!" + exceptionJson);
            Console.WriteLine();
        }

        public static void WriteInfo(string line)
        {
            Console.WriteLine("#" + stopwatcher?.Elapsed + ": " + line);
        }

        private static string ReadFromStdin()
        {
            var sb = new StringBuilder();
            string line;
            do
            {
                line = Console.ReadLine();
                sb.Append(line);
            } while (!string.IsNullOrEmpty(line));
            return sb.ToString();
        }
    }
}
