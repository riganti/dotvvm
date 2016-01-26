using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DotVVM.Compiler
{
    class Program
    {
        private static Stopwatch sw;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            if (args.Length == 0) while (true) DoCompileFromStdin();
            if(args[0] == "--json")
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

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            WriteInfo($"Assembly `{ args.Name }` resolve failed");
            if (args.Name.StartsWith(typeof(DotVVM.Framework.KnockoutHelper).Assembly.GetName().Name + ",", StringComparison.Ordinal)) return typeof(Framework.KnockoutHelper).Assembly;
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
            Console.WriteLine("#" + sw.Elapsed + ": " + line);
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
