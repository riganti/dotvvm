using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace DotVVM.Utils.ConfigurationHost.Extensions
{
    public static class XmlExtensions
    {
        public static XElement Descendant(this XContainer container, XName name)
        {
            return container.Descendants(name).FirstOrDefault();
        }
    }
}