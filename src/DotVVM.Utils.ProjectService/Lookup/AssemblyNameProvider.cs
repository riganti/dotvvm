using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using DotVVM.Utils.ConfigurationHost.Extensions;

namespace DotVVM.Utils.ConfigurationHost.Lookup
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