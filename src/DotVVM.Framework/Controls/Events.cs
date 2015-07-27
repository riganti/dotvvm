using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class Events
    {
        [AttachedProperty(typeof(object))]
        public static ActiveDotvvmProperty ClickProperty =
            DelegateActionProperty<object>.Register<Events>("Click", (writer, context, value, control) =>
            {
                var binding = value as CommandBindingExpression;
                var script = KnockoutHelper.GenerateClientPostBackScript(binding, context, control as DotvvmBindableControl);
                writer.AddAttribute("onclick", script);
            });
    }
}
