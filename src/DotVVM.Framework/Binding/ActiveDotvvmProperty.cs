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
    }
}
