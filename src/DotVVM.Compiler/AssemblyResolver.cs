using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace DotVVM.Compiler
{
    public class AssemblyResolver
    {
        internal static List<NugetDllMetadata> packagesDlls;
        internal static int isResolveRunning = 0;
        internal static readonly string[] supportedFrameworks = new[] { "\\net45\\", "\\net451\\", "\\net452\\", "\\net46\\", "\\net461\\", "\\net462\\" };
        internal static readonly string[] supportedFrameworksFragments = new[] { "net45", "net451", "net452", "net46", "net461", "net462" };

        internal static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            if (Interlocked.CompareExchange(ref isResolveRunning, 1, 0) != 0) return null;
            if (packagesDlls == null) LoadDotNetCoreAssemblies();
            try
            {
                Program2.WriteInfo($"Resolving assembly `{args.Name}`.");
                var r = LoadFromAlternativeFolder(args.Name);
                if (r != null) return r;
                Program2.WriteInfo($"Assembly `{args.Name}` resolve failed.");

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
            foreach (var path in Program2.assemblySearchPaths)
            {
                var assemblyPath = Path.Combine(path, new AssemblyName(name).Name + ".dll");
                if (!File.Exists(assemblyPath)) continue;
                return Assembly.LoadFile(assemblyPath);
            }

            var assemblyName = new AssemblyName(name);
            var packages = packagesDlls.Where(s =>
                string.Equals(s.PackageName, assemblyName.Name, StringComparison.OrdinalIgnoreCase) && assemblyName.Version == s.Version).ToList();

            if (packages.Any())
            {
                return Assembly.LoadFile(packages.First().Location);
            }
            return null;
        }


        private static void LoadDotNetCoreAssemblies()
        {
            var pathNuget = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget");
            var nugetCacheDir = new DirectoryInfo(pathNuget);

            if (!nugetCacheDir.Exists)
                throw new Exception("Nuget cache folder not found.");

            var dlls = nugetCacheDir.GetFiles("*.dll", SearchOption.AllDirectories);
            packagesDlls = dlls.ToList().Where(s => supportedFrameworks.Any(b => s.FullName.Contains(b))).Select(GetMetadata).ToList();

        }


        private static NugetDllMetadata GetMetadata(FileInfo fileInfo)
        {

            if (!fileInfo.Exists)
                return null;

            var meta = new NugetDllMetadata {
                Location = fileInfo.FullName,
                FileName = fileInfo.Name,
                DirectoryFragments = GetDirectoryFragments(fileInfo.Directory)
            };


            var fromPackages = meta.DirectoryFragments.TakeWhile(b => !b.Contains("packages")).ToList();
            meta.PackageName = fromPackages.Last();
            meta.PackageVersion = fromPackages[fromPackages.Count - 2];

            //assembly version 
            var pVer = meta.PackageVersion;
            if (pVer.All(s => char.IsDigit(s) || s == '.'))
            {
                var level = pVer.Count(s => s == '.');
                if (level == 2)
                {
                    pVer += ".0";
                }
                else if (level == 1)
                {
                    pVer += ".0.0";

                }
                meta.Version = new Version(pVer);
            }
            else
            {
                var version = string.Concat(pVer.TakeWhile(s => char.IsDigit(s) || s == '.'));
                version = version.EndsWith(".") ? version.Substring(0, version.Length - 1) : version;
                var level = version.Count(s => s == '.');
                if (level == 2)
                {
                    version += ".0";
                }
                else if (level == 1)
                {
                    version += ".0.0";

                }
                meta.Version = new Version(version);
            }

            meta.TargetFramework = supportedFrameworksFragments.FirstOrDefault(s => fromPackages.Any(b => b == s));
            return meta;
        }

        private static List<string> GetDirectoryFragments(DirectoryInfo fileInfoDirectory)
        {
            var list = new List<string>();

            void getDirectoryInfos(DirectoryInfo dir)
            {
                if (dir.Parent == null || (dir.Parent != null && dir.Parent.Parent == null)) return;
                list.Add(dir.Parent.Name);
                getDirectoryInfos(dir.Parent);
            }
            list.Add(fileInfoDirectory.Name);
            getDirectoryInfos(fileInfoDirectory);
            return list;
        }

        public static void LoadReferences(Assembly wsa)
        {
            foreach (var referencedAssembly in wsa.GetReferencedAssemblies())
            {
                if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.GetName().Name == referencedAssembly.Name) != null) continue;
                var assembly = Assembly.Load(referencedAssembly);
                LoadReferences(assembly);
            }
        }
    }

    internal class NugetDllMetadata
    {
        public Version Version { get; set; }
        public string Location { get; set; }
        public string TargetFramework { get; set; }
        public string FileName { get; set; }
        public List<string> DirectoryFragments { get; set; }
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
    }
}
