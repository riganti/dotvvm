using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    public class TranslateVirtualPathHtmlAttributeTransformer : IHtmlAttributeTransformer
    {
        public void RenderHtmlAttribute(IHtmlWriter writer, IDotvvmRequestContext requestContext, string attributeName, string attributeValue)
        {
            attributeValue = requestContext.TranslateVirtualPath(attributeValue);
            writer.WriteHtmlAttribute(attributeName, attributeValue);
        }
    }
}
