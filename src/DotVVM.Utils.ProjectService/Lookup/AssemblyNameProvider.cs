using System.IO;
using System.Xml.Linq;
using DotVVM.Utils.ProjectService.Extensions;

namespace DotVVM.Utils.ProjectService.Lookup
{
    public class AssemblyNameProvider
    {
        public string GetAssemblyName(XDocument xml, XNamespace ns, FileInfo file)
        {
            var assemblyNameElement = xml.Descendant(ns + "AssemblyName");
            if (assemblyNameElement != null)
            {
                return assemblyNameElement.Value;
            }
            return Path.GetFileNameWithoutExtension(file.FullName);
        }
    }
}
