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
            var projectDependenciesProvider = new ProjectDependenciesProvider();
            var targetFrameworkProvider = new TargetFrameworkProvider();
            var assemblyNameProvider = new AssemblyNameProvider();
            var dotvvmCompilerCompatibilityProvider = new DotvvmCompilerCompatibilityProvider();

            return allCsprojs.Select(file => {
                var xml = XDocument.Load(file.FullName);

                var ns = xml.Root?.GetDefaultNamespace();
                var csprojVersion = csprojVersionProvider.GetVersion(xml);

                var assetsFile = projectDependenciesProvider.GetProjectAssetsJson(file.DirectoryName);
                var dependencies = projectDependenciesProvider.GetDotvvmDependencies(xml, ns, csprojVersion, assetsFile);
                var targetFramework = targetFrameworkProvider.GetFramework(xml, ns, csprojVersion);
                var assemblyName = assemblyNameProvider.GetAssemblyName(xml, ns, file);
                var runCompiler = dotvvmCompilerCompatibilityProvider.IsCompatible(xml, ns, csprojVersion);
                var nugetFolder = NugetMetadataProvider.GetPackagesDirectories(assetsFile);
                var assemblyPath = ProjectOutputAssemblyProvider.GetAssemblyPath(file, assemblyName, targetFramework);
                var dotvvmPackageVersion = dependencies.FirstOrDefault(s => s.Name.Equals("DotVVM", StringComparison.OrdinalIgnoreCase) && !s.IsProjectReference);

                return new ResolvedProjectMetadata() {
                    CsprojVersion = csprojVersion,
                    TargetFramework = targetFramework,
                    CsprojFullName = file.FullName,
                    AssemblyName = assemblyName,
                    RunDotvvmCompiler = runCompiler,
                    DotvvmProjectDependencies = dependencies,
                    ProjectRootDirectory = file.DirectoryName,
                    AssemblyPath =  assemblyPath,
                    PackageNugetFolders = nugetFolder,
                    DotvvmPackageNugetFolders = dotvvmPackageVersion == null ? new List<string>() : nugetFolder.Select(s => Path.Combine(s, dotvvmPackageVersion.Name, dotvvmPackageVersion.Version)).ToList(),
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
