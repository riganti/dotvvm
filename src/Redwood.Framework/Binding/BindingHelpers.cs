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
        public static string[] GetBindingString(this RedwoodBindableControl control, RedwoodProperty property, bool translateToClientScript = false, bool inherit = true)
        {
            var expr = control.GetBinding(property, inherit) as ValueBindingExpression;
            if (expr == null) return null; 
            return expr.GetPath();
        }
    }
}
