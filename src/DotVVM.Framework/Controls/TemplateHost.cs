using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public ITemplate ContentTemplate { get; set; }


        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            ContentTemplate.BuildContent(context, this);
            base.OnLoad(context);
        }
    }
}
