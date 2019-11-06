#nullable enable
using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders different content to the users that are authenticated and users that are not.
    /// </summary>
    [ControlMarkupOptions(DefaultContentProperty = nameof(AuthenticatedTemplate))]
    public class AuthenticatedView : ConfigurableHtmlControl
    {
        /// <summary>
        /// Gets or sets the content displayed to the authenticated users.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate? AuthenticatedTemplate
        {
            get { return (ITemplate?)GetValue(AuthenticatedTemplateProperty); }
            set { SetValue(AuthenticatedTemplateProperty, value); }
        }
        public static readonly DotvvmProperty AuthenticatedTemplateProperty
            = DotvvmProperty.Register<ITemplate?, AuthenticatedView>(c => c.AuthenticatedTemplate, null);


        /// <summary>
        /// Gets or sets the content displayed to the users that are not authenticated.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate? NotAuthenticatedTemplate
        {
            get { return (ITemplate?)GetValue(NotAuthenticatedTemplateProperty); }
            set { SetValue(NotAuthenticatedTemplateProperty, value); }
        }
        public static readonly DotvvmProperty NotAuthenticatedTemplateProperty
            = DotvvmProperty.Register<ITemplate?, AuthenticatedView>(c => c.NotAuthenticatedTemplate, null);


        public AuthenticatedView() : base("div")
        {
            RenderWrapperTag = false;
        }

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            var isAuthenticated = context.HttpContext.User?.Identity?.IsAuthenticated;
            if (isAuthenticated == true)
            {
                AuthenticatedTemplate?.BuildContent(context, this);
            }
            else
            {
                NotAuthenticatedTemplate?.BuildContent(context, this);
            }
        }
    }
}
