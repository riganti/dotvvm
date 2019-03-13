using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DotVVM.Utils.ConfigurationHost.Lookup
{
    public class ProjectSystemSearcher
    {
        public IEnumerable<IResult> Search(AppConfiguration configuration)
        {
            var allCsprojs = new DirectoryInfo(configuration.LookupFolder).GetFiles("*.csproj", SearchOption.AllDirectories);
            var csprojVersionProvider = new CsprojVersionProvider();
            var dotvvmVersionProvider = new DotvvmVersionProvider();
            var targetFrameworkProvider = new TargetFrameworkProvider();
            var assemblyNameProvider = new AssemblyNameProvider();
            var dotvvmCompilerCompatibilityProvider = new DotvvmCompilerCompatibilityProvider();
            return
            allCsprojs.Select(file =>
            {
                var xml = XDocument.Load(file.FullName);
                var ns = xml.Root?.GetDefaultNamespace();
                var csprojVersion = csprojVersionProvider.GetVersion(xml);
                var packages = dotvvmVersionProvider.GetVersions(xml, ns, csprojVersion);
                var targetFramework = targetFrameworkProvider.GetFramework(xml, ns, csprojVersion);
                var assemblyName = assemblyNameProvider.GetAssemblyName(xml, ns, file);
                var runCompiler = dotvvmCompilerCompatibilityProvider.IsCompatible(xml, ns, csprojVersion);
                return !IsRequiredCsprojVersion(configuration, csprojVersion)
                    ? null
                    : new SearchResult()
                    {
                        CsprojVersion = csprojVersion,
                        TargetFramework = targetFramework,
                        CsprojFullName = file.FullName,
                        AssemblyName = assemblyName,
                        RunDotvvmCompiler = runCompiler,
                        DotvvmPackagesVersions = packages
                    };
            }).Where(s => s != null).ToList();
        }

        private static bool IsRequiredCsprojVersion(AppConfiguration configuration, CsprojVersion csprojVersion)
        {
            return csprojVersion == configuration.Version || configuration.Version == CsprojVersion.None;
        }
    }
}
