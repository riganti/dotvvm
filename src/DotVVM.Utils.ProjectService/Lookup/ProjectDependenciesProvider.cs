using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DotVVM.Utils.ProjectService.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Utils.ProjectService.Lookup
{
    public class ProjectDependenciesProvider
    {
        public List<ProjectDependency> GetDotvvmDependencies(XDocument xml, XNamespace ns, CsprojVersion csprojVersion, JObject assetsFile)
        {
            var versions = new List<ProjectDependency>();
            try
            {
                switch (csprojVersion)
                {
                    case CsprojVersion.OlderProjectSystem:
                        FillVersionsFromOldCsproj(xml, ns, versions);
                        return versions;
                    case CsprojVersion.DotNetSdk:
                        FillVersionsFromNewCsproj(xml, ns, versions, assetsFile);
                        return versions;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return versions;
        }

        private void FillVersionsFromOldCsproj(XDocument xml, XNamespace ns, List<ProjectDependency> versions)
        {
            var references = xml.Descendants(ns + "Reference");

            foreach (var reference in references)
            {
                var include = reference.Attribute("Include")?.Value;
                if (!IsDotvvmReference(include)) continue; // is null check as well

                // ReSharper disable once PossibleNullReferenceException
                var s = include.Split(',');
                var version = GetVersion(s);

                var name = s.Single(IsDotvvmReference).Trim();
                versions.Add(new ProjectDependency() {
                    Name = name,
                    Version = version
                });
            }

            var projectReferences = xml.Descendants(ns + "ProjectReference");
            foreach (var reference in projectReferences)
            {
                var include = reference.Attribute("Include")?.Value;
                if (!IsDotvvmReference(include)) continue;

                var name = GetDotvvmProjectReferenceName(include);
                if (name == null) continue;
                versions.Add(new ProjectDependency() {
                    Name = name,
                    IsProjectReference = true,
                });
            }
        }

        private string GetVersion(string[] s)
        {
            const string versionStr = "Version";
            return s.Single(a => a.Contains(versionStr, StringComparison.OrdinalIgnoreCase))
                .Replace(versionStr + "=", "").Trim();
        }

        private void FillVersionsFromNewCsproj(XDocument xml, XNamespace ns, List<ProjectDependency> projectDependencies,
            JObject assetsFile)
        {

            var packages = assetsFile["libraries"]
                .Children()
                .OfType<JProperty>()
                .Where(s => !string.IsNullOrWhiteSpace(s.Name) && s.Name.StartsWith("DotVVM", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var dependencies = packages.Select(s => {
                var parts = s.Name.Split('/');
                var isProjectReference =
                    (s.Value.Children<JProperty>().FirstOrDefault(b => b.Name == "type")?.Value as JValue)?.Value
                    ?.ToString() == "project";

                return new ProjectDependency() {
                    Name = parts[0],
                    Version = parts[1],
                    IsProjectReference = isProjectReference,
                    ProjectPath = (s.Value.Children<JProperty>().FirstOrDefault(b => b.Name == "path")?.Value as JValue)
                        ?.Value?.ToString(),
                };
            });

            foreach (var dependency in dependencies)
            {
                projectDependencies.Add(dependency);
            }
        }

        private bool IsDotvvmReference(string name)
        {
            return name?.Contains("dotvvm", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        private string GetDotvvmProjectReferenceName(string name)
        {
            if (name.EndsWith("\\DotVVM.Framework.csproj", StringComparison.OrdinalIgnoreCase))
            {
                return "DotVVM";
            }
            if (name.EndsWith("\\DotVVM.Framework.Hosting.Owin.csproj", StringComparison.OrdinalIgnoreCase))
            {
                return "DotVVM.Owin";
            }
            if (name.EndsWith("\\DotVVM.Framework.Hosting.AspNetCore.csproj", StringComparison.OrdinalIgnoreCase))
            {
                return "DotVVM.AspNetCore";
            }

            return null;
        }

        public JObject GetProjectAssetsJson(string projectFolder)
        {
            var project = new DirectoryInfo(projectFolder);
            var objs = project.GetDirectories("obj", SearchOption.TopDirectoryOnly);
            if (objs.Length != 1) return null;
            var assetsFile = objs.FirstOrDefault()?.GetFiles("project.assets.json", SearchOption.TopDirectoryOnly).FirstOrDefault();
            return assetsFile == null
                ? null
                : JObject.Load(new JsonTextReader(new StreamReader(assetsFile.FullName)));
        }
    }


}
