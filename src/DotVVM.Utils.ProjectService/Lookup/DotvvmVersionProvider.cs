using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DotVVM.Utils.ProjectService.Extensions;

namespace DotVVM.Utils.ProjectService.Lookup
{
    public class DotvvmVersionProvider
    {
        public List<PackageVersion> GetVersions(XDocument xml, XNamespace ns, CsprojVersion csprojVersion)
        {
            var versions = new List<PackageVersion>();
            try
            {
                switch (csprojVersion)
                {
                    case CsprojVersion.OlderProjectSystem:
                        FillVersionsFromOldCsproj(xml, ns, versions);
                        return versions;
                    case CsprojVersion.DotNetSdk:
                        GetVersionsFromNewCsproj(xml, ns, versions);
                        return versions;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return versions;
        }

        private void FillVersionsFromOldCsproj(XDocument xml, XNamespace ns, List<PackageVersion> versions)
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
                versions.Add(new PackageVersion() {
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
                versions.Add(new PackageVersion() {
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

        private void GetVersionsFromNewCsproj(XDocument xml, XNamespace ns, List<PackageVersion> versions)
        {
            var references = xml.Descendants(ns + "PackageReference");

            foreach (var reference in references)
            {
                var name = reference.Attribute("Include")?.Value;
                if (!IsDotvvmReference(name)) continue;
                var version = reference.Attribute("Version")?.Value;

                versions.Add(new PackageVersion() {
                    Name = name,
                    Version = version
                });
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
    }


}
