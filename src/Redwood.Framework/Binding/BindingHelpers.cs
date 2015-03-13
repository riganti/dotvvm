using Redwood.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Binding
{
    public static class BindingHelpers
    {
        public static string GetBindingString(this RedwoodBindableControl control, RedwoodProperty property, bool translateToClientScript = false, bool inherit = true)
        {
            var expr = control.GetBinding(property, inherit);
            if (expr == null) return null; 
            if (translateToClientScript) return expr.TranslateToClientScript(control, property);
            else return expr.Expression;
        }
    }
}
