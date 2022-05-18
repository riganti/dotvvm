using System;
using System.Linq;

namespace DotVVM.Framework.Controls.DynamicData.Annotations
{
    /// <summary>
    /// Show or hides the field based on the current view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ViewFilterAttribute : Attribute, IVisibilityFilter
    {
        /// <summary>
        /// Gets or sets the names of the views that this attribute applies to.
        /// </summary>
        public string[] ViewNames { get; }

        /// <summary>
        /// Gets or sets whether the field will be shown or hidden.
        /// </summary>
        public VisibilityMode Mode { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="ViewFilterAttribute" /> class.
        /// </summary>
        /// <param name="viewNames">Comma-separated list of views. The rule is matched if the current view is one of the values of this parameter.</param>
        /// <param name="mode">Specified whether the field should be shown or hidden when the rule is matched.</param>
        public ViewFilterAttribute(string viewNames, VisibilityMode mode = VisibilityMode.Show)
        {
            ViewNames = viewNames.Split(',', ';').Select(v => v.Trim()).ToArray();
            Mode = mode;
        }

        /// <summary>
        /// Evaluates whether the field should be shown or hidden. If this method returns null, the next filter attribute will be evaluated.
        /// </summary>
        public VisibilityMode? CanShow(IViewContext viewContext)
        {
            if (ViewNames.Contains(viewContext.ViewName, StringComparer.OrdinalIgnoreCase))
            {
                return Mode;
            }
            return null;
        }
    }
}
