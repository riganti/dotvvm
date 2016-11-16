using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DotVVM.Compiler
{
    /// <summary>
    /// References versions MUST match with reference versions on Dotvvm.Framework, or else compiler will not be able to load them.
    /// Project that will use Dotvvm will have to use at least the version stated in Dotvvm nuget.
    /// However if versions here are lower, there is no way to ensure them
    /// </summary>
    internal static class Program
    {
        public static List<string> AssemblySearchPaths = new List<string>();
        private static Stopwatch stopwatcher;

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            if (!AppDomain.CurrentDomain.ShadowCopyFiles)
            {
                var appDomain = AppDomain.CreateDomain("SecondaryDomainShadowCopyAllowed", null, new AppDomainSetup
                {
                    ShadowCopyFiles = "true",
                });
                appDomain.ExecuteAssemblyByName(typeof(Program).Assembly.FullName, args);
                return;
            }

            if (args.Length == 0)
            {
                while (true)
                {
                    var optionsJson = ReadJsonFromStdin();
                    DoCompile(GetCompilerOptions(optionsJson));
                }
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

        private static int isResolveRunning = 0;

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (Interlocked.CompareExchange(ref isResolveRunning, 1, 0) != 0) return null;
            try
            {
                var r = LoadFromAlternativeFolder(args.Name);
                if (r != null) return r;
                WriteInfo($"Assembly `{args.Name}` resolve failed");

                //We cannot do tyoepf(sometyhing).Assembly, because besides the compiler there are no dlls, dointhat will try to load the dll from the disk
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
            IEnumerable<string> paths = Environment.GetEnvironmentVariable("assemblySearchPath")?.Split(',') ?? new string[0];
            paths = paths.Concat(AssemblySearchPaths);
            foreach (var path in paths)
            {
                string assemblyPath = Path.Combine(path, new AssemblyName(name).Name + ".dll");
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
            WriteInfo("Starting");

            //Dont touch anything until the paths are filled we have to touch Json though
            try
            {
                var compiler = new ViewStaticCompilerCompiler();
                compiler.Options = options;

                var result = compiler.Execute();
                Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
                Console.WriteLine();
                return true;
            }
            catch (Exception ex)
            {
                WriteError(ex);
                return false;
            }
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
                    AssemblySearchPaths.Add(Path.GetDirectoryName(options.WebSiteAssembly));
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