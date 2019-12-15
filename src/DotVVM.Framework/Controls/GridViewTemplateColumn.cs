using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
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
        public ITemplate ContentTemplate
        {
            get { return (ITemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }
        public static readonly DotvvmProperty ContentTemplateProperty
            = DotvvmProperty.Register<ITemplate, GridViewTemplateColumn>(c => c.ContentTemplate, null);


        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate EditTemplate
        {
            get { return (ITemplate)GetValue(EditTemplateProperty); }
            set { SetValue(EditTemplateProperty, value); }
        }
        public static readonly DotvvmProperty EditTemplateProperty
            = DotvvmProperty.Register<ITemplate, GridViewTemplateColumn>(c => c.EditTemplate, null);


        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            ContentTemplate.BuildContent(context, container);
        }

        public override void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            if (EditTemplate == null) throw new DotvvmControlException(this, "EditTemplate must be set when editing is allowed in a GridView.");
            EditTemplate.BuildContent(context, container);
        }
    }
}
