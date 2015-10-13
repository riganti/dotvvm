using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Exceptions;

namespace DotVVM.Framework.Controls
{
    public class GridViewTemplateColumn : GridViewColumn
    {

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement, Required = true)]
        public ITemplate ContentTemplate { get; set; }

        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            ContentTemplate.BuildContent(context, container);
        }

        private IValueBinding GetValueBinding()
        {
            var binding = GetValueBinding(ValueBindingProperty);
            if (binding == null)
            {
                throw new DotvvmControlException(this, $"The ValueBinding property is not set on the '{GetType()}' control!");
            }
            return binding;
        }
    }

    
}
