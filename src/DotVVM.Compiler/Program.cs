using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Controls;

namespace DotVVM.Compiler
{
    static class Program
    {
        public static List<string> AssemblySearchPaths = new List<string>();
        private static Stopwatch sw;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            if (args.Length == 0) while (true) DoCompileFromStdin();
            if (args[0] == "--json")
            {
                var opt = string.Join(" ", args.Skip(1));
                if (!DoCompile(opt))
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Assembly GetDotvvmAssembly() => typeof(KnockoutHelper).Assembly;

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var r = LoadFromAlternativeFolder(args.Name);
            if (r != null) return r;
            WriteInfo($"Assembly `{ args.Name }` resolve failed");
            if (args.Name.StartsWith("DotVVM.Framework,", StringComparison.OrdinalIgnoreCase)) return GetDotvvmAssembly();

            return null;
        }

        static Assembly LoadFromAlternativeFolder(string name)
        {
            IEnumerable<string> paths = Environment.GetEnvironmentVariable("assemblySearchPath")?.Split(',') ?? new string[0];
            paths = paths.Concat(AssemblySearchPaths);
            foreach (var path in paths)
            {
                string assemblyPath = Path.Combine(path, new AssemblyName(name).Name + ".dll");
                if (!File.Exists(assemblyPath)) continue;
                return Assembly.LoadFrom(assemblyPath);
            }
            return null;
        }

        static bool DoCompileFromStdin()
        {
            var optionsJson = ReadFromStdin();
            if (string.IsNullOrWhiteSpace(optionsJson)) { Environment.Exit(0); return false; }
            return DoCompile(optionsJson);
        }

        static bool DoCompileFromFile(string file)
        {
            var optionsJson = File.ReadAllText(file);
            return DoCompile(optionsJson);
        }

        static bool DoCompile(string optionsJson)
        {
            sw = Stopwatch.StartNew();
            WriteInfo("Starting");
            try
            {
                var compiler = new ViewStaticCompilerCompiler();
                compiler.Options = JsonConvert.DeserializeObject<CompilerOptions>(optionsJson);
                if (!String.IsNullOrEmpty(compiler.Options.WebSiteAssembly))
                {
                    AssemblySearchPaths.Add(Path.GetDirectoryName(compiler.Options.WebSiteAssembly));
                }
                var result = compiler.Execute();
                Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
                Console.WriteLine();
                return true;
            }
            catch (Exception ex)
            {
                WriteInfo("Error occured!");
                var exceptionJson = JsonConvert.SerializeObject(ex);
                Console.WriteLine("!" + exceptionJson);
                Console.WriteLine();
                return false;
            }
        }

        public static void WriteInfo(string line)
        {
            Console.WriteLine("#" + sw?.Elapsed + ": " + line);
        }

        static string ReadFromStdin()
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
