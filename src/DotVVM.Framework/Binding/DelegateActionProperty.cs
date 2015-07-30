using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    public class DelegateActionProperty<TValue>: ActiveDotvvmProperty
    {
        private Action<IHtmlWriter, RenderContext, TValue, DotvvmControl> func;

        public DelegateActionProperty(Action<IHtmlWriter, RenderContext, TValue, DotvvmControl> func)
        {
            this.func = func;
        }

        public override void AddAttributesToRender(IHtmlWriter writer, RenderContext context, object value, DotvvmControl control)
        {
            if(func != null) func(writer, context, (TValue)value, control);
        }

        public static DelegateActionProperty<TValue> Register<TDeclaringType>(string name, Action<IHtmlWriter, RenderContext, TValue, DotvvmControl> func, TValue defaultValue = default(TValue))
        {
            return DotvvmProperty.Register<TValue, TDeclaringType>(name, defaultValue, false, new DelegateActionProperty<TValue>(func)) as DelegateActionProperty<TValue>;
        }

    }
}
