using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using DotVVM.Compiler.DTOs;
using DotVVM.Compiler.Programs;
using DotVVM.Framework.Utils;
#if NETCOREAPP2_0
using Microsoft.Extensions.DependencyModel;
using System.Runtime.Loader;
#endif

namespace DotVVM.Compiler.Resolving
{
    public class AssemblyResolver
    {

        static object nugetPakckagesLock = new object();
        static object dotnetStorePackagesLock = new object();
        internal static bool nugetPackagesDllsInited = false;
        internal static bool storeDllsInited = false;
        internal static ConcurrentBag<AssemblyFileMetadata> nugetPackagesDlls;
        internal static ConcurrentBag<AssemblyFileMetadata> dotnetStorePackagesDlls;
        internal static int isResolveRunning = 0;
        internal static readonly string[] supportedFrameworks = new[] { "\\net45\\", "\\net451\\", "\\net452\\", "\\net46\\", "\\net461\\", "\\net462\\", "\\net47\\" };
        internal static readonly string[] supportedFrameworksFragments = new[] { "net45", "net451", "net452", "net46", "net461", "net462", "net47" };
        public static DotNetCliInfo DotNetCliInfo { get; set; }

        internal static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            if (Interlocked.CompareExchange(ref isResolveRunning, 1, 0) != 0) return null;

            try
            {
                Program2.WriteInfo($"Resolving assembly `{args.Name}`.");
                var r = LoadFromAlternativeFolder(args.Name);
                if (r != null) return r;
                Program2.WriteInfo($"Assembly `{args.Name}` resolve failed.");

                //We cannot do typeof(something).Assembly, because besides the compiler there are no dlls, doing that will try to load the dll from the disk
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
            if (TryLoadAssemblyFromUserFolders(name, out var loadAssemblyFromFile)) return loadAssemblyFromFile;


#if NETCOREAPP2_0
            if (nugetPackagesDllsInited == false) LoadDotNetCoreNugetAssemblies();

            var assemblyName = new AssemblyName(name);
            var packages = nugetPackagesDlls.Where(s =>
                string.Equals(s.PackageName, assemblyName.Name, StringComparison.OrdinalIgnoreCase) && assemblyName.Version == s.Version).ToList();

            if (packages.Any())
            {
                return LoadAssemblyFromFile(packages.First().Location);
            }

            if (storeDllsInited == false) LoadDotNetStoreAssemblies();
            var storePackages = dotnetStorePackagesDlls.Where(s =>
                string.Equals(s.PackageName, assemblyName.Name, StringComparison.OrdinalIgnoreCase) && assemblyName.Version == s.Version).ToList();

            if (storePackages.Any())
            {
                return LoadAssemblyFromFile(storePackages.First().Location);
            }
#endif
            return null;
        }
        /// <summary>
        /// Tries to find and load assembly from folder specified in options and environment variable at the start of the app.
        /// </summary>
        private static bool TryLoadAssemblyFromUserFolders(string name, out Assembly loadAssemblyFromFile)
        {
            foreach (var path in Program2.assemblySearchPaths)
            {
                var assemblyPath = Path.Combine(path, new AssemblyName(name).Name);

                if (File.Exists(assemblyPath + ".dll"))
                {
                    {
                        loadAssemblyFromFile = LoadAssemblyFromFile(assemblyPath + ".dll");
                        return true;
                    }
                }

                if (File.Exists(assemblyPath + ".exe"))
                {
                    {
                        loadAssemblyFromFile = LoadAssemblyFromFile(assemblyPath + ".exe");
                        return true;
                    }
                }
            }

            loadAssemblyFromFile = null;
            return false;
        }

        private static Assembly LoadAssemblyFromFile(string assemblyPath)
        {
            return AssemblyLoader.LoadFile(assemblyPath);

        }

        private static void LoadDotNetStoreAssemblies()
        {
            lock (dotnetStorePackagesLock)
            {
                if (storeDllsInited)
                {
                    return;
                }

                storeDllsInited = true;
                if (DotNetCliInfo == null)
                {
                    dotnetStorePackagesDlls = new ConcurrentBag<AssemblyFileMetadata>();
                    return;
                }

                var storePath = Path.Combine(DotNetCliInfo.Store, DetermineStoreTargetFrameworkFolderName());
                var storeDir = new DirectoryInfo(storePath);

                if (!storeDir.Exists) return;

                var dlls = storeDir.GetFiles("*.dll", SearchOption.AllDirectories);
                dotnetStorePackagesDlls = new ConcurrentBag<AssemblyFileMetadata>(dlls.Select(s => GetDotnetStoreMetadata(storeDir, s)).Where(s => s != null).ToList());
            }
        }

        private static string DetermineStoreTargetFrameworkFolderName()
        {
            //TODO: this should be resolved from deps.json file
            return "netcoreapp2.0";
        }


        private static void LoadDotNetCoreNugetAssemblies()
        {
            lock (nugetPakckagesLock)
            {
                if (nugetPackagesDllsInited)
                {
                    return;
                }

                nugetPackagesDllsInited = true;
                var pathNuget = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget");
                var nugetCacheDir = new DirectoryInfo(pathNuget);

                if (!nugetCacheDir.Exists)
                    throw new Exception("Nuget cache folder not found.");

                var dlls = nugetCacheDir.GetFiles("*.dll", SearchOption.AllDirectories);
                nugetPackagesDlls = new ConcurrentBag<AssemblyFileMetadata>(dlls.Where(s => supportedFrameworks.Any(b => s.FullName.Contains(b))).Select(fileInfo => GetNugetMetadata(fileInfo)).Where(s => s != null).ToList());
            }
        }


        private static AssemblyFileMetadata GetNugetMetadata(FileInfo fileInfo)
        {

            if (!fileInfo.Exists)
                return null;

            var meta = new AssemblyFileMetadata {
                Location = fileInfo.FullName,
                FileName = fileInfo.Name,
                DirectoryFragments = GetDirectoryFragments(fileInfo.Directory)
            };


            var fromPackages = meta.DirectoryFragments.TakeWhile(b => !b.Contains("packages")).ToList();
            meta.PackageName = fromPackages.Last();
            meta.PackageVersion = fromPackages[fromPackages.Count - 2];

            //assembly version 
            meta.Version = GetPackageVersion(meta);

            meta.TargetFramework = supportedFrameworksFragments.FirstOrDefault(s => fromPackages.Any(b => b == s));
            return meta;
        }
        /// <summary>
        /// Try to parse package verion (from path/PackageVersion)
        /// </summary>
        private static Version GetPackageVersion(AssemblyFileMetadata meta)
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

        private static Version CreateNewVersion(string version)
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

        /// <summary>
        /// Get all packages in dotnet core store 
        /// </summary>
        private static AssemblyFileMetadata GetDotnetStoreMetadata(DirectoryInfo storeDir, FileInfo fileInfo)
        {

            if (!fileInfo.Exists)
                return null;

            var meta = new AssemblyFileMetadata {
                Location = fileInfo.FullName,
                FileName = fileInfo.Name,
                DirectoryFragments = GetDirectoryFragments(fileInfo.Directory, storeDir)
            };


            meta.PackageName = meta.DirectoryFragments.Last();
            meta.PackageVersion = meta.DirectoryFragments[meta.DirectoryFragments.Count - 2];

            //assembly version 
            meta.Version = GetPackageVersion(meta);

            //TODO: this should be only mark that this assembly is for netcore
            meta.TargetFramework = "netstandard2.0";
            return meta;
        }
        private static List<string> GetDirectoryFragments(DirectoryInfo fileInfoDirectory, DirectoryInfo baseDir = null)
        {
            var list = new List<string>();

            void getDirectoryInfos(DirectoryInfo dir)
            {
                if (dir.Parent == null || (dir.Parent != null && dir.Parent.Parent == null) || dir.Parent.FullName == baseDir?.FullName) return;
                list.Add(dir.Parent.Name);
                getDirectoryInfos(dir.Parent);
            }
            list.Add(fileInfoDirectory.Name);
            getDirectoryInfos(fileInfoDirectory);
            return list;
        }

        public static void LoadReferencedAssemblies(Assembly wsa, bool recursive = false)
        {
            foreach (var referencedAssembly in wsa.GetReferencedAssemblies())
            {
                if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.GetName().Name == referencedAssembly.Name) != null) continue;
                var assembly = Assembly.Load(referencedAssembly);
                if (recursive)
                    LoadReferencedAssemblies(assembly);
            }
        }
    }
}
