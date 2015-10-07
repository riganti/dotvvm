using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    public abstract class ActiveDotvvmProperty: DotvvmProperty
    {
        public abstract void AddAttributesToRender(IHtmlWriter writer, RenderContext context, object value, DotvvmControl control);


        public static ActiveDotvvmProperty RegisterCommandToAttribute<TDeclaringType>(string name, string attributeName)
        {
            return DelegateActionProperty<object>.Register<TDeclaringType>(name, (writer, context, value, control) =>
            {
                var binding = value as ICommandBinding;
                var script = KnockoutHelper.GenerateClientPostBackScript(binding, context, control as DotvvmBindableControl);
                writer.AddAttribute(attributeName, script);
            });
        }
    }
}
