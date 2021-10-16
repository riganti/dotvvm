using System;
using System.Linq;
using System.Security.Claims;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders different content to the users who have a specified claim and to users who haven't.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(HasClaimTemplate))]
    public class ClaimView : ConfigurableHtmlControl
    {
        public ClaimView()
            : base("div")
        {
            RenderWrapperTag = false;
        }

        /// <summary>
        /// Gets or sets the type of claim the user must have.
        /// </summary>
        [MarkupOptions(Required = true)]
        public string? Claim
        {
            get { return (string?)GetValue(ClaimProperty); }
            set { SetValue(ClaimProperty, value); }
        }

        public static readonly DotvvmProperty ClaimProperty
            = DotvvmProperty.Register<string?, ClaimView>(c => c.Claim);

        /// <summary>
        /// Gets or sets a comma-separated list of accepted values. If specified; the user must have the <see cref="Claim" />
        /// with one or more of the values. Otherwise; all values are accepted.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string[]? Values
        {
            get { return (string[]?)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        public static readonly DotvvmProperty ValuesProperty
            = DotvvmProperty.Register<string[]?, ClaimView>(c => c.Values);

        /// <summary>
        /// Gets or sets the content displayed to the users who have the <see cref="Claim" /> with one or more of accepted values.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement, AllowBinding = false)]
        public ITemplate? HasClaimTemplate
        {
            get { return (ITemplate?)GetValue(HasClaimTemplateProperty); }
            set { SetValue(HasClaimTemplateProperty, value); }
        }

        public static readonly DotvvmProperty HasClaimTemplateProperty
            = DotvvmProperty.Register<ITemplate?, ClaimView>(c => c.HasClaimTemplate);

        /// <summary>
        /// Gets or sets the content displayed to the users who don't have the <see cref="Claim" /> with any of accepted values.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement, AllowBinding = false)]
        public ITemplate? HasNotClaimTemplate
        {
            get { return (ITemplate?)GetValue(HasNotClaimTemplateProperty); }
            set { SetValue(HasNotClaimTemplateProperty, value); }
        }

        public static readonly DotvvmProperty HasNotClaimTemplateProperty
            = DotvvmProperty.Register<ITemplate?, ClaimView>(c => c.HasNotClaimTemplate);

        /// <summary>
        /// Gets or sets whether the control will be hidden completely to anonymous users. If set to <c>false</c>,
        /// the <see cref="HasNotClaimTemplate" /> will be rendered to anonymous users.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool HideForAnonymousUsers
        {
            get { return (bool)GetValue(HideForAnonymousUsersProperty)!; }
            set { SetValue(HideForAnonymousUsersProperty, value); }
        }

        public static readonly DotvvmProperty HideForAnonymousUsersProperty
            = DotvvmProperty.Register<bool, ClaimView>(c => c.HideForAnonymousUsers, true);

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            var user = context.HttpContext.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated == true;

            if (!HideForAnonymousUsers || isAuthenticated)
            {
                if (isAuthenticated && HasClaim(user))
                {
                    HasClaimTemplate?.BuildContent(context, this);
                }
                else
                {
                    HasNotClaimTemplate?.BuildContent(context, this);
                }
            }
        }

        private bool HasClaim(ClaimsPrincipal? user)
        {
            if (user != null)
            {
                return Values?.Any(v => user.HasClaim(Claim, v.Trim()))
                    ?? user.HasClaim(ClaimIsOfRequiredType);
            }

            return false;
        }

        private bool ClaimIsOfRequiredType(Claim claim)
            => string.Equals(claim.Type, Claim, StringComparison.OrdinalIgnoreCase);
    }
}
