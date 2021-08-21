using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a template supplied by a resource binding or from a runtime.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class TemplateHost : DotvvmControl
    {

        /// <summary>
        /// Gets or sets the template that will be rendered inside this control.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.Attribute, Required = true)]
        public ITemplate Template
        {
            get { return (ITemplate)GetValue(TemplateProperty); }
            set { SetValue(TemplateProperty, value); }
        }
        public static readonly DotvvmProperty TemplateProperty
            = DotvvmProperty.Register<ITemplate, TemplateHost>(c => c.Template, null);



        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            Template.BuildContent(context, this);
            base.OnLoad(context);
        }
    }
}
