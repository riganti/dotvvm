using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{

    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(ContentTemplate))]
    public class GridViewTemplateColumn : GridViewColumn
    {

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement, Required = true)]
        public ITemplate ContentTemplate { get; set; }

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate EditTemplate { get; set; }

        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            ContentTemplate.BuildContent(context, container);
        }

        public override void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            EditTemplate.BuildContent(context, container);
        }
    }
}
