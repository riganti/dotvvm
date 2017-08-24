using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace DotVVM.Framework.Controls.DynamicData.Annotations
{
    /// <summary>
    /// Show or hides the field based on current user role memebership.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RoleFilterAttribute : Attribute, IVisibilityFilter
    {
        /// <summary>
        /// Gets or sets the names of the roles that this attribute applies to.
        /// </summary>
        public string[] RoleNames { get; }

        /// <summary>
        /// Gets or sets whether the field will be shown or hidden.
        /// </summary>
        public VisibilityMode Mode { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="RoleFilterAttribute" /> class.
        /// </summary>
        /// <param name="roleNames">Comma-separated list of roles. The rule is matched if the user is in any of the roles.</param>
        /// <param name="mode">Specified whether the field should be shown or hidden when the user is in any of the roles.</param>
        public RoleFilterAttribute(string roleNames, VisibilityMode mode = VisibilityMode.Show)
        {
            Mode = mode;
            RoleNames = roleNames.Split(',', ';').Select(s => s.Trim()).ToArray();
        }

        /// <summary>
        /// Evaluates whether the field should be shown or hidden. If this method returns null, the next filter attribute will be evaluated.
        /// </summary>
        public VisibilityMode? CanShow(IViewContext viewContext)
        {
            if (viewContext.CurrentUser != null && RoleNames.Any(viewContext.CurrentUser.IsInRole))
            {
                return Mode;
            }
            return null;
        }

    }
}
