using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    public class CompiledBindingExpression
    {
        public string OriginalString { get; set; }
        public string Javascript { get; set; }
        public Expression Expression { get; set; }
        public BindingDelegate Delegate { get; set; }
        public BindingUpdateDelegate UpdateDelegate { get; set; }
        public string Id { get; set; }
        public ActionFilterAttribute[] ActionFilters { get; set; }
        public Dictionary<string, object> Extensions { get; set; }

        public delegate object BindingDelegate(object[] dataContextHierarchy, DotvvmBindableObject rootControl);
        public delegate void BindingUpdateDelegate(object[] dataContextHierarchy, DotvvmBindableObject rootControl, object value);
    }
}
