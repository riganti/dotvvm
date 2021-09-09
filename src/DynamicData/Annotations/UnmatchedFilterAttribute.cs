using System;

namespace DotVVM.Framework.Controls.DynamicData.Annotations
{
    /// <summary>
    /// Specifies whether the field should be shown or hidden if none of the filter attributes is matched.
    /// </summary>
    public class UnmatchedFilterAttribute : Attribute, IVisibilityFilter
    {
        /// <summary>
        /// Gets or sets whether the field will be shown or hidden.
        /// </summary>
        public VisibilityMode Mode { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ViewFilterAttribute" /> class.
        /// </summary>
        /// <param name="mode">Specified whether the field should be shown or hidden.</param>
        public UnmatchedFilterAttribute(VisibilityMode mode)
        {
            Mode = mode;
        }

        /// <summary>
        /// Evaluates whether the field should be shown or hidden. If this method returns null, the next filter attribute will be evaluated.
        /// </summary>
        public VisibilityMode? CanShow(IViewContext viewContext)
        {
            return Mode;
        }
    }
}