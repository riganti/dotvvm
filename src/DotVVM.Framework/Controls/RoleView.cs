using System.Linq;
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
        /// Gets or sets whether the control will be hidden completely to users which are not authenticated.
        /// If set to false, the IsNotMemberTemplate will be rendered to non-authenticated users.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool HideNonAuthenticatedUsers
        {
            get { return (bool)GetValue(HideNonAuthenticatedUsersProperty); }
            set { SetValue(HideNonAuthenticatedUsersProperty, value); }
        }
        public static readonly DotvvmProperty HideNonAuthenticatedUsersProperty
            = DotvvmProperty.Register<bool, RoleView>(c => c.HideNonAuthenticatedUsers, true);


        public RoleView() : base("div")
        {
            RenderWrapperTag = false;
        }


        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            var isAuthenticated = context.OwinContext.Request?.User?.Identity?.IsAuthenticated;
            if (isAuthenticated == true || !HideNonAuthenticatedUsers)
            {
                var isMember = Roles?
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Any(r => context.OwinContext.Request?.User?.IsInRole(r.Trim()) == true);

                if (isMember == true)
                {
                    IsMemberTemplate?.BuildContent(context, this);
                }
                else
                {
                    IsNotMemberTemplate?.BuildContent(context, this);
                }
            }
        }
    }
}