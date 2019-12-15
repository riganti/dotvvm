using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Binding.Expressions
{
    public delegate object BindingDelegate(object[] dataContextHierarchy, DotvvmBindableObject rootControl);
    public delegate T BindingDelegate<out T>(object[] dataContextHierarchy, DotvvmBindableObject rootControl);
    public delegate void BindingUpdateDelegate(object[] dataContextHierarchy, DotvvmBindableObject rootControl, object value);
    public delegate void BindingUpdateDelegate<in T>(object[] dataContextHierarchy, DotvvmBindableObject rootControl, T value);
}
