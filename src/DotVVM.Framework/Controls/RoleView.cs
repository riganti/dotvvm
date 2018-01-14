using System.Linq;
using System.Security.Claims;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders different content to the users which are in a specified role and users which are not.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(IsMemberTemplate))]
    public class RoleView : ConfigurableHtmlControl
    {
        public RoleView()
            : base("div")
        {
            RenderWrapperTag = false;
        }

        /// <summary>
        /// Gets or sets a comma-separated list of roles. The user must be a member of one or more of these roles.
        /// </summary>
        [MarkupOptions(AllowBinding = false, Required = true)]
        public string[] Roles
        {
            get { return (string[])GetValue(RolesProperty); }
            set { SetValue(RolesProperty, value); }
        }

        public static readonly DotvvmProperty RolesProperty
            = DotvvmProperty.Register<string[], RoleView>(c => c.Roles, null);

        /// <summary>
        /// Gets or sets the content displayed to the users which are in one or more roles specified by the Roles property.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement, AllowBinding = false)]
        public ITemplate IsMemberTemplate
        {
            get { return (ITemplate)GetValue(IsMemberTemplateProperty); }
            set { SetValue(IsMemberTemplateProperty, value); }
        }

        public static readonly DotvvmProperty IsMemberTemplateProperty
            = DotvvmProperty.Register<ITemplate, RoleView>(c => c.IsMemberTemplate, null);

        /// <summary>
        /// Gets or sets the content displayed to the users which are not in any of the roles specified by the Roles property.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement, AllowBinding = false)]
        public ITemplate IsNotMemberTemplate
        {
            get { return (ITemplate)GetValue(IsNotMemberTemplateProperty); }
            set { SetValue(IsNotMemberTemplateProperty, value); }
        }

        public static readonly DotvvmProperty IsNotMemberTemplateProperty
            = DotvvmProperty.Register<ITemplate, RoleView>(c => c.IsNotMemberTemplate, null);

        /// <summary>
        /// Gets or sets whether the control will be hidden completely to anonymous users. If set to <c>false</c>,
        /// the <see cref="IsNotMemberTemplate" /> will be rendered to anonymous users.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool HideForAnonymousUsers
        {
            get { return (bool)GetValue(HideForAnonymousUsersProperty); }
            set { SetValue(HideForAnonymousUsersProperty, value); }
        }

        public static readonly DotvvmProperty HideForAnonymousUsersProperty
            = DotvvmProperty.Register<bool, RoleView>(c => c.HideForAnonymousUsers, true);

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            var user = context.HttpContext.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated == true;

            if (!HideForAnonymousUsers || isAuthenticated)
            {
                if (isAuthenticated && IsMember(user))
                {
                    IsMemberTemplate?.BuildContent(context, this);
                }
                else
                {
                    IsNotMemberTemplate?.BuildContent(context, this);
                }
            }
        }

        private bool IsMember(ClaimsPrincipal user)
        {
            if (user != null && Roles != null)
            {
                return Roles.Any(r => user.IsInRole(r.Trim()));
            }

            return false;
        }
    }
}
