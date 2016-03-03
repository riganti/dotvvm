using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Binding.Expressions
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
