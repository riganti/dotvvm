using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Binding
{
    public abstract class ActiveDotvvmProperty: DotvvmProperty
    {
        public abstract void AddAttributesToRender(IHtmlWriter writer, RenderContext context, DotvvmControl control);


        public static ActiveDotvvmProperty RegisterCommandToAttribute<TDeclaringType>(string name, string attributeName)
        {
            return DelegateActionProperty<ICommandBinding>.Register<TDeclaringType>(name, (writer, context, property, control) =>
            {
                var binding = control.GetCommandBinding(property);
                var script = KnockoutHelper.GenerateClientPostBackScript(name, binding, context, control);
                writer.AddAttribute(attributeName, script);
            });
        }
    }
}
