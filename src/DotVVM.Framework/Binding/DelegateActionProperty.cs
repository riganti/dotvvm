#nullable enable
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
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
        private Action<IHtmlWriter, IDotvvmRequestContext, DotvvmProperty, DotvvmControl> func;

        public DelegateActionProperty(Action<IHtmlWriter, IDotvvmRequestContext, DotvvmProperty, DotvvmControl> func)
        {
            this.func = func;
        }

        public override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context, DotvvmControl control)
        {
            if(func != null) func(writer, context, this, control);
        }

        public static DelegateActionProperty<TValue> Register<TDeclaringType>(string name, Action<IHtmlWriter, IDotvvmRequestContext, DotvvmProperty, DotvvmControl> func, TValue defaultValue = default(TValue))
        {
            return (DelegateActionProperty<TValue>)DotvvmProperty.Register<TValue, TDeclaringType>(name, defaultValue, false, new DelegateActionProperty<TValue>(func));
        }

    }
}
