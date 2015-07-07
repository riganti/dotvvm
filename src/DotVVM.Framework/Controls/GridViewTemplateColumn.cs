using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    public class GridViewTemplateColumn : GridViewColumn
    {

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate ContentTemplate
        {
            get { return (ITemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }
        public static readonly DotvvmProperty ContentTemplateProperty =
            DotvvmProperty.Register<ITemplate, GridViewTemplateColumn>(c => c.ContentTemplate);



        public override void CreateControls(DotvvmRequestContext context, DotvvmControl container)
        {
            ContentTemplate.BuildContent(context, container);
        }

        private ValueBindingExpression GetValueBinding()
        {
            var binding = GetValueBinding(ValueBindingProperty);
            if (binding == null)
            {
                throw new Exception(string.Format("The ValueBinding property is not set on the {0} control!", GetType()));
            }
            return binding;
        }
    }

    
}
