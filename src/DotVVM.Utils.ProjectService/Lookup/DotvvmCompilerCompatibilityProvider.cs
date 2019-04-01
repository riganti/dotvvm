using System.Linq;
using System.Xml.Linq;
using DotVVM.Utils.ProjectService.Extensions;

namespace DotVVM.Utils.ProjectService.Lookup
{
    public class DotvvmCompilerCompatibilityProvider
    {
        public bool IsCompatible(XDocument xml, XNamespace ns, CsprojVersion csprojVersion)
        {
            switch (csprojVersion)
            {
                case CsprojVersion.DotNetSdk:
                    return false; //TODO: Logic for deciding compatible projects
                case CsprojVersion.OlderProjectSystem:
                    return IsCompatibleOldCsproj(xml, ns);
                default:
                    return false;
            }
        }

        private bool IsCompatibleOldCsproj(XDocument xml, XNamespace ns)
        {
            var guids = xml.Descendant(ns + "ProjectTypeGuids");
            if (guids == null) return false;

            return guids.Value.Split(';').Contains(Constants.DotvvmProjectGuid);
        }
    }
}
