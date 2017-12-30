using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;

namespace DotVVM.Compiler
{
    public class Program2
    {

        private static HashSet<string> assemblySearchPaths = new HashSet<string>();
        private static Stopwatch stopwatcher;

        public static void ContinueMain(string[] args)
        {
            GetEnvironmentAssemblySearchPaths();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

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

        private static int isResolveRunning = 0;

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (Interlocked.CompareExchange(ref isResolveRunning, 1, 0) != 0) return null;
            try
            {
                WriteInfo($"Resolving assembly `{args.Name}`.");
                var r = LoadFromAlternativeFolder(args.Name);
                if (r != null) return r;
                WriteInfo($"Assembly `{args.Name}` resolve failed.");

                //We cannot do typeof(sometyhing).Assembly, because besides the compiler there are no dlls, dointhat will try to load the dll from the disk
                //which fails, so this event is called again call this event...

                return null;
            }
            finally
            {
                isResolveRunning = 0;
            }
        }

        private static Assembly LoadFromAlternativeFolder(string name)
        {
            foreach (var path in assemblySearchPaths)
            {
                var assemblyPath = Path.Combine(path, new AssemblyName(name).Name + ".dll");
                if (!File.Exists(assemblyPath)) continue;
                return Assembly.LoadFile(assemblyPath);
            }
            return null;
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
                    result = DoFullCompile(options);
                }
                else
                {
                    // only export configuration
                    result = ExportConfiguration(options);
                }
                Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings {
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

        private static CompilationResult DoFullCompile(CompilerOptions options)
        {
            var compiler = new ViewStaticCompilerCompiler();
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
