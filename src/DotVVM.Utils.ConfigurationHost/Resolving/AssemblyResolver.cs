using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using DotVVM.Compiler.DTOs;
using DotVVM.Utils.ConfigurationHost;
using System.Data;
using DotVVM.Utils.ProjectService.Lookup;
using Microsoft.CodeAnalysis;
using DotVVM.Framework.Hosting;
#if NETSTANDARD
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using System.Runtime.Loader;
#endif

namespace DotVVM.Compiler.Resolving
{
    public class AssemblyResolver
    {
        internal static int isResolveRunning = 0;
        private string compilationConf;
        private TargetFramework target;

        private IEnumerable<string> assemblySearchPaths { get; set; }
        private ILogger logger { get; set; }


        public AssemblyResolver(HashSet<string> assemblySearchPaths, ILogger logger, string compilationConf, TargetFramework target)
        {
            this.assemblySearchPaths = assemblySearchPaths;
            this.logger = logger;
            this.compilationConf = compilationConf;
            this.target = target;
        }

        public Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            if (Interlocked.CompareExchange(ref isResolveRunning, 1, 0) != 0) return null;

            try
            {
                var r = LoadFromAlternativeFolder(args.Name, compilationConf, target);
                if (r != null) return r;
                //We cannot do typeof(something).Assembly, because besides the compiler there are no dlls, doing that will try to load the dll from the disk
                //which fails, so this event is called again call this event...

                return null;
            }
            finally
            {
                isResolveRunning = 0;
            }
        }

        private Assembly LoadFromAlternativeFolder(string name, string compilationConf, Utils.ProjectService.Lookup.TargetFramework target)
        {
            if (TryLoadAssemblyFromUserFolders(name, out var loadAssemblyFromFile, compilationConf, target)) return loadAssemblyFromFile;

            return null;
        }
        /// <summary>
        /// Tries to find and load assembly from folder specified in options and environment variable at the start of the app.
        /// </summary>
        private bool TryLoadAssemblyFromUserFolders(string name, out Assembly loadAssemblyFromFile, string compilationConf, Utils.ProjectService.Lookup.TargetFramework target)
        {

            foreach (var path in assemblySearchPaths)
            {
                var assemblySearchPath = new DirectoryInfo(Path.Combine(path, compilationConf, target.TranslateToFolderName()));
                if (!assemblySearchPath.Exists)
                    assemblySearchPath = new DirectoryInfo(path);

                if (assemblySearchPath.Exists)
                {

                    var file = assemblySearchPath.GetFiles($"{new AssemblyName(name).Name}.dll", SearchOption.AllDirectories).FirstOrDefault();
                    if (file?.Exists ?? false)
                    {
                        loadAssemblyFromFile = LoadAssemblyFromFile(file.FullName);

                        return true;
                    }
                    file = assemblySearchPath.GetFiles($"{new AssemblyName(name).Name}.exe", SearchOption.AllDirectories).FirstOrDefault();
                    if (file?.Exists ?? false)
                    {
                        loadAssemblyFromFile = LoadAssemblyFromFile(file.FullName);

                        return true;
                    }
                }
            }

            loadAssemblyFromFile = null;
            return false;
        }

        private Assembly LoadAssemblyFromFile(string assemblyPath)
        {
            return AssemblyLoader.LoadFile(assemblyPath);
        }





        /// <summary>
        /// Try to parse package verion (from path/PackageVersion)
        /// </summary>
        private Version GetPackageVersion(AssemblyFileMetadata meta)
        {
            //To keep this app lightweight, do not add reference to nuget packages.

            //parse stable versions
            var pVer = meta.PackageVersion;
            if (pVer.All(s => char.IsDigit(s) || s == '.'))
            {
                return CreateNewVersion(pVer);
            }
            else
            {
                //skip the suffix
                var version = string.Concat(pVer.TakeWhile(s => char.IsDigit(s) || s == '.'));
                version = version.EndsWith(".") ? version.Substring(0, version.Length - 1) : version;
                return CreateNewVersion(version);
            }
        }

        private Version CreateNewVersion(string version)
        {
            var level = version.Count(s => s == '.');
            if (level == 2)
            {
                version += ".0";
            }
            else if (level == 1)
            {
                version += ".0.0";
            }

            return new Version(version);
        }



        public void LoadReferencedAssemblies(Assembly wsa, bool recursive = false)
        {
            foreach (var referencedAssembly in wsa.GetReferencedAssemblies())
            {
                if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.GetName().Name == referencedAssembly.Name) != null) continue;
                var assembly = Assembly.Load(referencedAssembly);
                if (recursive)
                    LoadReferencedAssemblies(assembly);
            }
        }
#if NETSTANDARD
        public void ResolverNetstandard(string webSiteAssemblyPath, string compilationConf, Utils.ProjectService.Lookup.TargetFramework target)
        {
            if (!File.Exists(webSiteAssemblyPath)) throw new ArgumentException("WebSiteAssemblyPath environment argument contains path to DLL.");

            var currentAssembly = Assembly.GetEntryAssembly();

            var asm = AssemblyLoader.LoadFile(webSiteAssemblyPath);

            var type = typeof(CompilationLibrary);

            var resolvers = assemblySearchPaths.Select(s => new AppBaseCompilationAssemblyResolver(s)).Concat(
                new ICompilationAssemblyResolver[]
                 {
                        new AppBaseCompilationAssemblyResolver(),
                        new ReferenceAssemblyPathResolver(),
                        new PackageCompilationAssemblyResolver(),
                        new ReferenceAssemblyPathResolver(@"C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App", new []{ @"C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All" })

                 }).ToArray();

            var composite = new CompositeCompilationAssemblyResolver(resolvers);
            type.GetFields(BindingFlags.Static | BindingFlags.NonPublic)[0].SetValue(null, composite);

            var dependencyContext = DependencyContext.Load(asm);


            var assemblyNames = ResolveAssemblies(dependencyContext, compilationConf, target);

            AssemblyLoadContext.Default.Resolving += (context, name) => {
                // find potential assemblies
                var assembly = assemblyNames
                    .Where(a => string.Equals(a.AssemblyFileName, name.Name, StringComparison.CurrentCultureIgnoreCase))
                    .Select(a => new { AssemblyData = a, AssemblyName = AssemblyLoadContext.GetAssemblyName(a.AssemblyFullPath) })
                    .FirstOrDefault(a => a.AssemblyName.Name == name.Name && a.AssemblyName.Version == name.Version);

                if (assembly == null)
                {
                    logger.LogInfo($"Resolving failed: {name.Name}/{name.Version}");
                    return null;
                }
                else
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(assembly.AssemblyData.AssemblyFullPath);
                }
            };
        }
        private ConcurrentBag<AssemblyData> ResolveAssemblies(DependencyContext dependencyContext, string compilationConf, TargetFramework target)
        {

            return new ConcurrentBag<AssemblyData>(dependencyContext.CompileLibraries
                .SelectMany(l => {
                    try
                    {
                        var paths = l.ResolveReferencePaths();
                        return paths.Select(p => new AssemblyData {
                            Library = l,
                            AssemblyFullPath = p,
                            AssemblyFileName = Path.GetFileNameWithoutExtension(p)
                        });
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Assembly a;
                            if (TryLoadAssemblyFromUserFolders(l.Name, out a, compilationConf, target))
                            {
                                return new List<AssemblyData>(){ new AssemblyData {
                                    Library = l,
                                    AssemblyFullPath = a.Location,
                                    AssemblyFileName = Path.GetFileNameWithoutExtension(a.Location)
                                } };
                            }
                            return Enumerable.Empty<AssemblyData>();
                        }
                        catch (Exception e)
                        {
                            return Enumerable.Empty<AssemblyData>();
                        }
                    }
                })
                .ToList());
        }
    }
    internal class AssemblyData
    {
        public CompilationLibrary Library { get; set; }
        public string AssemblyFullPath { get; set; }
        public string AssemblyFileName { get; set; }

#endif
    }
}
