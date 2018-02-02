using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;

namespace DotVVM.Compiler
{

    public class Program2
    {


        internal static CompilerOptions Options { get; private set; }
        internal static HashSet<string> assemblySearchPaths { get; private set; } = new HashSet<string>();
        private static Stopwatch stopwatcher;

        public static void ContinueMain(string[] args)
        {
            GetEnvironmentAssemblySearchPaths();
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver.ResolveAssembly;

            if (args.Length == 0)
            {
                while (true)
                {
                    var optionsJson = ReadJsonFromStdin();
                    Options = GetCompilerOptions(optionsJson);
                    DoCompile(Options);
                }
            }
            if (args[0] == "-?")
            {
                WriteHelp();
                Exit(0);
            }
            if (args[0] == "--debugger")
            {
                WaitForDbg();
                args = args.Skip(1).ToArray();
            }

            if (args[0] == "--json")
            {
                var opt = string.Join(" ", args.Skip(1));

                Options = GetCompilerOptions(opt);
                if (!DoCompile(Options))
                {
                    Exit(1);
                }
            }
            else
            {
                var file = string.Join(" ", args);
                if (!DoCompileFromFile(file))
                {
                    Exit(1);
                }
            }

        }

        private static void WriteHelp()
        {
            Console.Write(@"
DotVVM Compiler
    --json      - Determines options for compiler
    --debugger  - Waits as long as compiler is not attached


JSON structure:
        string[] DothtmlFiles           (null = build all, [] = build none)
        string WebSiteAssembly          
        bool OutputResolvedDothtmlMap   
        string BindingsAssemblyName 
        string BindingClassName 
        string OutputPath 

        string AssemblyName             
        string WebSitePath              
        bool FullCompile                (default: true)
        bool CheckBindingErrors         
        bool SerializeConfig            
        string ConfigOutputPath

");

        }

        private static void Exit(int exitCode)
        {
            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }
            Environment.Exit(1);
        }

        private static void GetEnvironmentAssemblySearchPaths()
        {
            assemblySearchPaths.Add(Environment.CurrentDirectory);
            foreach (var path in Environment.GetEnvironmentVariable("assemblySearchPath")?.Split(',') ?? new string[0])
            {
                assemblySearchPaths.Add(path);
            }
        }

        private static void WaitForDbg()
        {
            WriteInfo("Process ID: " + Process.GetCurrentProcess().Id);
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

            //Dont touch anything until the paths are filled we have to touch Json though
            try
            {
                CompilationResult result;
                if (options.FullCompile)
                {
                    // compile views
                    WriteInfo("Starting full compilation...");
                    result = DoFullCompile(options);
                }
                else
                {
                    // only export configuration
                    WriteInfo("Starting export configuration only ...");
                    result = ExportConfiguration(options);
                }

                var serializedResult = JsonConvert.SerializeObject(result, Formatting.Indented,
                    new JsonSerializerSettings {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                Console.WriteLine(serializedResult);
                WriteConfigurationOutput(serializedResult);

                Console.WriteLine();
                return true;
            }
            catch (Exception ex)
            {
                WriteError(ex);
                return false;
            }
        }

        private static void WriteConfigurationOutput(string serializedResult)
        {
            var path = Options.ConfigOutputPath;
            if (string.IsNullOrWhiteSpace(path)) return;
            var file = new FileInfo(path);
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            WriteInfo($"Saving configuration to '{file.FullName}'");
            File.WriteAllText(file.FullName, serializedResult);
        }

        private static CompilationResult DoFullCompile(CompilerOptions options)
        {
            var compiler = new ViewStaticCompiler();
            compiler.Options = options;
            return compiler.Execute();
        }
        private static CompilationResult ExportConfiguration(CompilerOptions options)
        {
            var assembly = Assembly.LoadFile(options.WebSiteAssembly);
            var config = OwinInitializer.InitDotVVM(assembly, options.WebSitePath, null, (s) => { });
            return new CompilationResult() {
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
                if (!string.IsNullOrEmpty(options.WebSiteAssembly))
                {
                    assemblySearchPaths.Add(Path.GetDirectoryName(options.WebSiteAssembly));
                }

                WriteInfo("Using the following assembly search paths: ");
                foreach (var path in assemblySearchPaths)
                {
                    WriteInfo(path);
                }
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
