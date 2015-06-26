using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    public interface IHtmlAttributeTransformer
    {

        /// <summary>
        /// Renders the attribute name and value into a specified writer.
        /// </summary>
        void RenderHtmlAttribute(IHtmlWriter writer, RedwoodRequestContext requestContext, string attributeName, string attributeValue);

    }
}