using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using DotVVM.Compiler.Compilation;
using DotVVM.Compiler.DTOs;
using DotVVM.Compiler.Fakes;
using DotVVM.Compiler.Resolving;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime.Caching;
using DotVVM.Framework.Security;
using DotVVM.Utils.ConfigurationHost;
using DotVVM.Utils.ConfigurationHost.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace DotVVM.Compiler.Programs
{
    public class Program2
    {
        internal static CompilerOptions Options { get; private set; }
        internal static HashSet<string> assemblySearchPaths { get; private set; } = new HashSet<string>();
        private static Stopwatch stopwatcher;
        private static string GetEnvironmentWebAssemblyPath()
        {
            return Environment.GetEnvironmentVariable(CompilerConstants.EnvironmentVariables.WebAssemblyPath);
        }
        public static void ContinueMain(string[] args)
        {
            WriteTargetFramework();

            GetEnvironmentAssemblySearchPaths();
            var assemblyResolver = new AssemblyResolver(assemblySearchPaths, new ConsoleLogger());
#if NETCOREAPP2_0
            assemblyResolver.ResolverNetstandard(GetEnvironmentWebAssemblyPath());
#else
            AppDomain.CurrentDomain.AssemblyResolve += assemblyResolver.ResolveAssembly;
#endif
            if (args.Length == 0)
            {
                while (true)
                {
                    var optionsJson = ReadJsonFromStdin();
                    Options = GetCompilerOptions(optionsJson);
                    ProcessCompilationRequest(Options);
                }
            }
            if (args[0] == CompilerConstants.Arguments.Help)
            {
                WriteHelp();
                Exit(0);
            }
            if (args[0] ==CompilerConstants.Arguments.WaitForDebugger)
            {
                WaitForDbg();
                args = args.Skip(1).ToArray();
            }
            if (args[0] == CompilerConstants.Arguments.WaitForDebuggerAndBreak)
            {
                WaitForDbg(true);
                args = args.Skip(1).ToArray();
            }

            if (args[0] == CompilerConstants.Arguments.JsonOptions)
            {
                var opt = string.Join(" ", args.Skip(1));

                Options = GetCompilerOptions(opt);
                if (!ProcessCompilationRequest(Options))
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

        private static void WriteTargetFramework()
        {

#if NET47
            WriteInfo("Target framework: .NET Framework 4.7");
#elif NET461
            WriteInfo("Target framework: .NET Framework 4.6");
#elif NETCOREAPP2_0
            WriteInfo("Target framework: .NET Standard 2.0");
#endif
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

        internal static void Exit(int exitCode)
        {
            Console.WriteLine($"#$ Exit {exitCode} - DotVVM Compiler Ended");
            Environment.Exit(exitCode);
        }

        private static void GetEnvironmentAssemblySearchPaths()
        {
            foreach (var path in Environment.GetEnvironmentVariable(CompilerConstants.EnvironmentVariables.AssemblySearchPath)?.Split(',') ?? new string[0])
            {
                assemblySearchPaths.Add(path);
            }
            assemblySearchPaths.Add(Environment.CurrentDirectory);
        }


        private static void WaitForDbg(bool _break = false)
        {
            WriteInfo("Process ID: " + Process.GetCurrentProcess().Id);
            while (!Debugger.IsAttached) Thread.Sleep(32);
            if (_break)
            {
                Debugger.Break();
            }
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
            return ProcessCompilationRequest(compilerOptions);
        }

        private static bool ProcessCompilationRequest(CompilerOptions options)
        {
            InitStopwacher();

            //Don't touch anything until the paths are filled we have to touch Json though
            try
            {
                CompilationResult result;
                if (options.FullCompile)
                {
                    // compile views
                    WriteInfo("Starting full compilation...");
                    result = Compile(options);
                }
                else if (options.CheckBindingErrors)
                {
                    // check errors views
                    WriteInfo("Starting error validation...");
                    result = Compile(options);
                }
                else
                {
                    // only export configuration
                    WriteInfo("Starting export configuration only ...");
                    result = ExportConfiguration(options);
                }

                ConfigurationSerialization.PreInit();

                var serializedResult = JsonConvert.SerializeObject(result, Formatting.Indented,
                    new JsonSerializerSettings {
                        TypeNameHandling = TypeNameHandling.Auto,
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

        private static CompilationResult Compile(CompilerOptions options)
        {
            var compiler = new ViewStaticCompiler();
            compiler.Options = options;
            return compiler.Execute();
        }
        private static CompilationResult ExportConfiguration(CompilerOptions options)
        {
            var assembly = Assembly.LoadFile(options.WebSiteAssembly);
            var config = ConfigurationInitializer.InitDotVVM(assembly, options.WebSitePath, null, collection => { });
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

        internal static void WriteError(Exception ex)
        {
            WriteInfo("Error occured!");
            var exceptionJson = JsonConvert.SerializeObject(ex);
            Console.WriteLine("! " + exceptionJson);
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

    internal class ConfigurationInitializer
    {
        public static DotvvmConfiguration InitDotVVM(Assembly assembly, string webSitePath, ViewStaticCompiler viewStaticCompiler, Action<IServiceCollection> additionalServices)
        {
            return ConfigurationHost.InitDotVVM(assembly, webSitePath, services => {

                if (viewStaticCompiler != null)
                {
                    services.AddSingleton<ViewStaticCompiler>(viewStaticCompiler);
                    services.AddSingleton<IControlResolver, OfflineCompilationControlResolver>();
                    services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>();
                    services.AddSingleton(new RefObjectSerializer());
                    services.AddSingleton<IDotvvmCacheAdapter, DotVVM.Framework.Testing.SimpleDictionaryCacheAdapter>();
                }

                additionalServices?.Invoke(services);
            });
        }
    }

    public class ConsoleLogger : ILogger
    {
        public void LogInfo(string message) => Program2.WriteInfo(message);


        public void LogException(Exception exception) => Program2.WriteError(exception);

        public void LogError(string message) => Program2.WriteInfo("[error]: " + message);
    }
}
