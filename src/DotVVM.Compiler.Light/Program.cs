using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Comp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                while (true)
                {
                    var optionsJson = ReadFromStdin();
                    if (string.IsNullOrWhiteSpace(optionsJson)) { Environment.Exit(0); }
                    OutputConfig(optionsJson);
                }
            }
            var optionString = GetOptionString(args);
            if (string.IsNullOrEmpty(optionString))
            {
                Environment.Exit(1);
            }
            if (!OutputConfig(optionString))
            {
                Environment.Exit(1);
            }

        }

        private static bool OutputConfig(string optionsJson)
        {
            try
            {
                var options = JsonConvert.DeserializeObject<CompilerOptions>(optionsJson);
                if (string.IsNullOrEmpty(options.WebSitePath) || !Directory.Exists(options.WebSitePath))
                {
                    Console.WriteLine("Error occured!");
                    Console.WriteLine($"Website directory not found: {options.WebSitePath}");
                    return false;
                }
                if (string.IsNullOrEmpty(options.WebSiteAssembly) || !File.Exists(options.WebSiteAssembly))
                {
                    Console.WriteLine("Error occured!");
                    Console.WriteLine($"Website directory not found: {options.WebSitePath}");
                    return false;
                }
                if (options.FullCompile)
                {
                    Console.WriteLine("Error: this compiler does not support full compile. It is unsupported by .Net Core 1.");
                    return false;
                }
                if (!options.SerializeConfig)
                {
                    Console.WriteLine("Error: This compiler is for serializing the configuration only! If you dont want thatm, you need full featured compiler.");
                    return false;
                }
                var config = GetDotvvmConfig(options.WebSiteAssembly, options.WebSitePath);
                var result = new CompilationResult() { Configuration = config };
                Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
                Console.WriteLine();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured!");
                var exceptionJson = JsonConvert.SerializeObject(ex);
                Console.WriteLine("!" + exceptionJson);
                Console.WriteLine();
                return false;
            }
        }

        public static string GetOptionString(string[] args)
        {
            if (args[0] == "--json")
            {
                return string.Join(" ", args.Skip(1));
            }
            else
            {
                var file = string.Join(" ", args);
                return File.ReadAllText(file);
            }
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

        public static DotvvmConfiguration GetDotvvmConfig(string assemblyOutputPath, string webSitePath)
        {
            var asl = new AssemblyLoader();

            Assembly outputAssembly = null;
            using (var stream = new FileStream(assemblyOutputPath, FileMode.Open))
            {
                outputAssembly = asl.LoadFromStream(stream);
            }

            var dotvvmStartups = outputAssembly.GetLoadableTypes()
                .Where(t => typeof(IDotvvmStartup).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null).ToArray();

            if (dotvvmStartups.Length == 0) throw new Exception("Could not find any implementation of IDotvvmStartup.");
            if (dotvvmStartups.Length > 1) throw new Exception($"Found more than one implementation of IDotvvmStartup ({string.Join(", ", dotvvmStartups.Select(s => s.Name)) }).");

            var startup = (IDotvvmStartup)Activator.CreateInstance(dotvvmStartups[0]);
            var config = DotvvmConfiguration.CreateDefault();
            startup.Configure(config, webSitePath);
            config.CompiledViewsAssemblies = null;
            return config;
        }

        public class AssemblyLoader : AssemblyLoadContext
        {
            // Not exactly sure about this
            protected override Assembly Load(AssemblyName assemblyName)
            {
                var deps = DependencyContext.Default;
                var res = deps.CompileLibraries.Where(d => d.Name.Contains(assemblyName.Name)).ToList();
                var assembly = Assembly.Load(new AssemblyName(res.First().Name));
                return assembly;
            }
        }

        public class CompilationResult
        {
            public DotvvmConfiguration Configuration { get; set; }
        }

        public class CompilerOptions
        {
            public string WebSiteAssembly { get; set; }
            public string WebSitePath { get; set; }
            public bool FullCompile { get; set; } = true;
            public bool SerializeConfig { get; set; }
        }
    }
}
