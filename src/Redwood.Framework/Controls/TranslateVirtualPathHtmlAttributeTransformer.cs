using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    public class TranslateVirtualPathHtmlAttributeTransformer : IHtmlAttributeTransformer
    {
        public void RenderHtmlAttribute(IHtmlWriter writer, RedwoodRequestContext requestContext, string attributeName, string attributeValue)
        {
            attributeValue = requestContext.TranslateVirtualPath(attributeValue);
            writer.WriteHtmlAttribute(attributeName, attributeValue);
        }
    }
}
