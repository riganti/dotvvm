using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls.DynamicData
{
    public static class ControlHelpers
    {

        public static void CopyProperty(DotvvmBindableObject source, DotvvmProperty sourceProperty, DotvvmBindableObject target, DotvvmProperty targetProperty)
        {
            var binding = source.GetValueBinding(sourceProperty);
            if (binding != null)
            {
                target.SetBinding(targetProperty, binding);
            }
            else
            {
                target.SetValue(targetProperty, source.GetValue(sourceProperty));
            }
        }

    }
}
