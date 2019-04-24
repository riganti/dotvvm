using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DotVVM.Utils.ProjectService.Lookup
{
    public class ProjectSystemProvider
    {
        public IEnumerable<IResolvedProjectMetadata> GetProjectMetadata(string lookupFolder)
        {
            var allCsprojs = FindProjects(lookupFolder);
            var csprojVersionProvider = new CsprojVersionProvider();
            var dotvvmVersionProvider = new DotvvmVersionProvider();
            var targetFrameworkProvider = new TargetFrameworkProvider();
            var assemblyNameProvider = new AssemblyNameProvider();
            var dotvvmCompilerCompatibilityProvider = new DotvvmCompilerCompatibilityProvider();
            return
            allCsprojs.Select(file => {
                var xml = XDocument.Load(file.FullName);
                var ns = xml.Root?.GetDefaultNamespace();
                var csprojVersion = csprojVersionProvider.GetVersion(xml);
                var packages = dotvvmVersionProvider.GetVersions(xml, ns, csprojVersion);
                var targetFramework = targetFrameworkProvider.GetFramework(xml, ns, csprojVersion);
                var assemblyName = assemblyNameProvider.GetAssemblyName(xml, ns, file);
                var runCompiler = dotvvmCompilerCompatibilityProvider.IsCompatible(xml, ns, csprojVersion);
                return new ResolvedProjectMetadata() {
                    CsprojVersion = csprojVersion,
                    TargetFramework = targetFramework,
                    CsprojFullName = file.FullName,
                    AssemblyName = assemblyName,
                    RunDotvvmCompiler = runCompiler,
                    DotvvmPackagesVersions = packages,
                    ProjectRootDirectory = Path.GetDirectoryName(file.FullName),
                    //TODO
                    AssemblyPath = ((string)null) ?? throw new NotImplementedException(),
                    DotvvmPackageNugetFolder = ((string)null) ?? throw new NotImplementedException(),
                };
            }).ToList();
        }

        public FileInfo[] FindProjects(string lookupFolder)
        {
            var allCsprojs = new DirectoryInfo(lookupFolder).GetFiles("*.csproj", SearchOption.AllDirectories);
            return allCsprojs;
        }
    }
}
