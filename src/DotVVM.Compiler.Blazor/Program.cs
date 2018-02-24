using System;
using System.Linq;
using System.Runtime.Loader;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;
using DotVVM.Framework.Configuration;

namespace DotVVM.Compiler.Blazor
{
    class Program
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
            else if (args[0] == "build")
            {
                var webSiteAssembly = args[1];
                var clientSiteAssembly = args[2];
                var webSitePath = args[3];
                var options = new CompilerOptions() {
                    FullCompile = true,
                    DothtmlFiles = null,
                    WebSitePath = webSitePath,
                    ClientSiteAssembly = clientSiteAssembly,
                    WebSiteAssembly = webSiteAssembly,
                    OutputPath = Path.GetDirectoryName(webSiteAssembly),
                    AssemblyName = Path.GetFileNameWithoutExtension(clientSiteAssembly) + ".DotvvmClientApp"
                };
                DoCompile(options);
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
            options.InitMissingOptions();

            //Dont touch anything until the paths are filled we have to touch Json though
            // try
            // {
                var config = GetConfiguration(options);

                var compiler = new Dothtml2BlazorCompiler(config, options);
                var result = compiler.Execute();

                Console.WriteLine();
                return true;
            // }
            // catch (Exception ex)
            // {
            //     WriteError(ex);
            //     return false;
            // }
        }

        private static DotvvmConfiguration GetConfiguration(CompilerOptions options)
        {
            var webSiteAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(options.WebSiteAssembly);
            var clientSiteAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(options.ClientSiteAssembly);
            var config = AspNetCoreInitializer.InitDotVVM(webSiteAssembly, clientSiteAssembly, options.WebSitePath, options.OutputPath, (s) => { });
            return config;
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
            Console.WriteLine("!" + ex);
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
