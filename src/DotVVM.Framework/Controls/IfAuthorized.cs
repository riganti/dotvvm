using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the Template (default content) iff the user is authorized and has specified roles
    /// </summary>
    [ControlMarkupOptions(DefaultContentProperty = nameof(Template))]
    public class IfAuthorized : DotvvmControl
    {
        /// <summary>
        /// This is rendered iff user is authorized and has specified roles
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate Template { get; set; }

        /// <summary>
        /// This is rendered iff user is not logged or does not have specified roles
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate ElseTemplate { get; set; }

        /// <summary>
        /// Roles must have to render the template for authorized
        /// </summary>
        public string[] Roles
        {
            get { return (string[])GetValue(RolesProperty); }
            set { SetValue(RolesProperty, value); }
        }
        public static readonly DotvvmProperty RolesProperty =
            DotvvmProperty.Register<string[], IfAuthorized>(t => t.Roles);

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            var authorized = context.OwinContext.Request.User != null &&
                context.OwinContext.Request.User.Identity.IsAuthenticated &&
                (Roles?.Any(context.OwinContext.Authentication.User.IsInRole) ?? true);
            if (authorized)
            {
                Template?.BuildContent(context, this);
            }
            else
            {
                ElseTemplate?.BuildContent(context, this);
            }
        }
    }
}
