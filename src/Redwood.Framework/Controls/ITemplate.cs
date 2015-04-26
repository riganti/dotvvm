using Redwood.Framework.Hosting;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Represents a markup template.
    /// </summary>
    public interface ITemplate
    {

        /// <summary>
        /// Builds the content of the template into the specified container.
        /// </summary>
        void BuildContent(RedwoodRequestContext context, RedwoodControl container);

    }
}