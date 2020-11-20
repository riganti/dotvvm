using System.Xml.Linq;

namespace DotVVM.Utils.ProjectService.Lookup
{
    public class CsprojVersionProvider
    {
        public CsprojVersion GetVersion(XDocument xml)
        {
            var projectElement = xml.Root;

            if (!string.IsNullOrWhiteSpace(projectElement?.Attribute("Sdk")?.Value))
            {
                return CsprojVersion.DotNetSdk;
            }
            if (!string.IsNullOrWhiteSpace(projectElement?.Attribute("xmlns")?.Value))
            {
                return CsprojVersion.OlderProjectSystem;
            }

            return CsprojVersion.None;
        }
    }
}
