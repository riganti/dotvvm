using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
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
        public ITemplate? ContentTemplate
        {
            get { return (ITemplate?)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }
        public static readonly DotvvmProperty ContentTemplateProperty
            = DotvvmProperty.Register<ITemplate?, GridViewTemplateColumn>(c => c.ContentTemplate, null);

        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            ContentTemplate.NotNull("GridViewTemplateColumn.ContentTemplate must be set")
                           .BuildContent(context, container);
        }
    }
}
