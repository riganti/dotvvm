using System.Linq;
using System.Xml.Linq;

namespace DotVVM.Utils.ProjectService.Extensions
{
    public static class XmlExtensions
    {
        public static XElement Descendant(this XContainer container, XName name)
        {
            return container.Descendants(name).FirstOrDefault();
        }
    }
}