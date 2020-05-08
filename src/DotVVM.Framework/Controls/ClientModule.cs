using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Controls
{
    [ControlMarkupOptions(AllowContent = true, DefaultContentProperty = nameof(Code))]
    public class ClientModule : DotvvmControl
    {

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public string Code
        {
            get { return (string)GetValue(CodeProperty); }
            set { SetValue(CodeProperty, value); }
        }
        public static readonly DotvvmProperty CodeProperty
            = DotvvmProperty.Register<string, ClientModule>(c => c.Code, null);

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            var resourceName = (string)GetValue(Internal.ClientModuleResourceNameProperty);
            if (resourceName != null)
            {
                context.ResourceManager.AddRequiredResource(ResourceConstants.DotvvmClientModuleResourceNamePrefix + ":" + resourceName);
            }
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
        }
    }
}
