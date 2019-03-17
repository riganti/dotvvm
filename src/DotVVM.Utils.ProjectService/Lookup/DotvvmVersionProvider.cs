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
                        GetVersionsFromOldCsproj(xml, ns, versions);
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

        private void GetVersionsFromOldCsproj(XDocument xml, XNamespace ns, List<PackageVersion> versions)
        {
            var references = xml.Descendants(ns + "Reference");

            foreach (var reference in references)
            {
                var include = reference.Attribute("Include")?.Value;
                if (!IsDotvvmReference(include)) continue; // is null check as well
                var s = include.Split(',');
                var version = GetVersion(s);
                var name = s.Single(IsDotvvmReference).Trim();
                versions.Add(new PackageVersion()
                {
                    Name = name,
                    Version = version
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

                versions.Add(new PackageVersion()
                {
                    Name = name,
                    Version = version
                });
            }
        }

        private bool IsDotvvmReference(string name)
        {
            return name.Contains("dotvvm", StringComparison.OrdinalIgnoreCase);
        }
    }
}