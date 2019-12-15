using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a markup template.
    /// </summary>
    public interface ITemplate
    {

        /// <summary>
        /// Builds the content of the template into the specified container.
        /// </summary>
        void BuildContent(IDotvvmRequestContext context, DotvvmControl container);

    }
}