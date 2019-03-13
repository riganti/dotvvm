using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DotVVM.Utils.ConfigurationHost.Extensions;

namespace DotVVM.Utils.ConfigurationHost.Lookup
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
                    return TargetFramework.NetCore;
            }
        }

        private TargetFramework GetFrameworkFromNewCsproj(XDocument xml, XNamespace ns)
        {
            var target = xml.Descendant(ns + "TargetFramework");
            if (target != null)
            {
                return Regex.IsMatch(target.Value, "^net\\d{2,3}$")
                    ? TargetFramework.NetFramework
                    : TargetFramework.NetCore;
            }

            target = xml.Descendant(ns + "TargetFrameworks");
            if (target != null)
            {
                return target.Value.Split(';').Any(t => Regex.IsMatch(t, "^net(coreapp|standard)\\d\\.\\d$"))
                    ? TargetFramework.NetCore
                    : TargetFramework.NetFramework;
            }

            return TargetFramework.NetCore;
        }
    }
}