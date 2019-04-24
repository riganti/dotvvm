using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DotVVM.Utils.ProjectService.Extensions;

namespace DotVVM.Utils.ProjectService.Lookup
{
    public class TargetFrameworkProvider
    {
        public TargetFramework GetFramework(XDocument xml, XNamespace ns, CsprojVersion csprojVersion)
        {
            switch (csprojVersion)
            {
                case CsprojVersion.OlderProjectSystem:
                    return TargetFramework.NetFramework;
                case CsprojVersion.DotNetSdk:
                    return GetFrameworkFromNewCsproj(xml, ns);
                default:
                    return TargetFramework.NetStandard;
            }
        }

        private TargetFramework GetFrameworkFromNewCsproj(XDocument xml, XNamespace ns)
        {
            var target = xml.Descendant(ns + "TargetFramework");
            if (target != null)
            {
                return ConvertTargetsIntoEnum(new List<string> { target.Value }).FirstOrDefault();
            }

            target = xml.Descendant(ns + "TargetFrameworks");
            if (target != null)
            {
                return ConvertTargetsIntoEnum(target.Value.Split(';').ToList()).FirstOrDefault();
            }
            return TargetFramework.NetStandard;
        }

        private List<TargetFramework> ConvertTargetsIntoEnum(List<string> values)
        {
            var names = Enum.GetNames(typeof(TargetFramework));
            return values.Select(s => s.Replace(".", "").Trim())
                        .Select(s => names.FirstOrDefault(b => b.Equals(s, StringComparison.OrdinalIgnoreCase)))
                        .Where(s => s is object)
                        .Select(s => { Enum.TryParse<TargetFramework>(s, out var a); return a; }).ToList();
        }

    }
}
