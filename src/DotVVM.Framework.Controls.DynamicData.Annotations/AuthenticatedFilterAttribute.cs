using System;
using System.Security.Claims;

namespace DotVVM.Framework.Controls.DynamicData.Annotations
{
    /// <summary>
    /// Show or hides the field based on current user authentication status.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AuthenticatedFilterAttribute : Attribute, IVisibilityFilter
    {
        /// <summary>
        /// Gets or sets whether the field will be shown or hidden.
        /// </summary>
        public VisibilityMode Mode { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="AuthenticatedFilterAttribute" /> class.
        /// </summary>
        /// <param name="mode">Specified whether the field should be shown or hidden when the user is authenticated.</param>
        public AuthenticatedFilterAttribute(VisibilityMode mode = VisibilityMode.Show)
        {
            Mode = mode;
        }

        /// <summary>
        /// Evaluates whether the field should be shown or hidden. If this method returns null, the next filter attribute will be evaluated.
        /// </summary>
        public VisibilityMode? CanShow(IViewContext viewContext)
        {
            if (viewContext.CurrentUser?.Identity?.IsAuthenticated == true)
            {
                return Mode;
            }
            return null;
        }
    }
}