using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Build.Framework;
using NuGet.Frameworks;

namespace DotVVM.Cli
{
    public class WriteDotvvmMetadataTask : ITask
    {
        public IBuildEngine? BuildEngine { get; set; }
        public ITaskHost? HostObject { get; set; }
        public string? TargetFrameworksString { get; set; }
        public string? AssemblyName { get; set; }
        public string? RootNamespace { get; set; }
        public string? ProjectFilePath { get; set; }
        [Required]
        public string? MetadataFilePath { get; set; }
        public ITaskItem[]? References { get; set; }
        public ITaskItem[]? PackageReferences { get; set; }

        public bool Execute()
        {
            if (MetadataFilePath is null)
            {
                return false;
            }

            var targetFrameworks = TargetFrameworksString is object
                ? GetTargetFrameworks(TargetFrameworksString)
                : null;
            var metadataJson = new ProjectMetadataJson
            {
                AssemblyName = AssemblyName,
                RootNamespace = RootNamespace,
                ProjectFilePath = ProjectFilePath,
                TargetFrameworks = targetFrameworks,
                PackageVersion = GetPackageVersion(References, PackageReferences)
            };
            var json = JsonSerializer.Serialize(metadataJson, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(MetadataFilePath, json);
            return true;
        }

        public static List<string> GetTargetFrameworks(string targetFrameworksString)
        {
            return targetFrameworksString.Trim()
                .Split(';')
                .Select(t => NuGetFramework.Parse(t).GetShortFolderName())
                .ToList();
        }

        public static string GetPackageVersion(
            IEnumerable<ITaskItem>? references,
            IEnumerable<ITaskItem>? packageReferences)
        {
            references ??= Enumerable.Empty<ITaskItem>();
            packageReferences ??= Enumerable.Empty<ITaskItem>();

            var reference = references
                .Select(r => new AssemblyName(r.ItemSpec))
                .FirstOrDefault(n => n.Name == DotvvmProject.AssemblyName);
            if (reference is object && reference.Version is object)
            {
                return reference.Version.ToString();
            }

            var package = packageReferences
                .FirstOrDefault(p => p.ItemSpec == DotvvmProject.PackageName);
            if (package is object)
            {
                return package.GetMetadata("Version");
            }

            return DotvvmProject.FallbackVersion;
        }
    }
}
