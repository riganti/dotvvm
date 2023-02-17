using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Binding.Expressions
{
    public delegate object? BindingDelegate(DotvvmBindableObject rootControl);
    public delegate T BindingDelegate<out T>(DotvvmBindableObject rootControl);
    public delegate void BindingUpdateDelegate(DotvvmBindableObject rootControl, object? value);
    public delegate void BindingUpdateDelegate<in T>(DotvvmBindableObject rootControl, T value);
}
