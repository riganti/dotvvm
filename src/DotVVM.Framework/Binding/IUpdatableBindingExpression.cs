using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public interface IUpdatableBindingExpression
    {

        void UpdateSource(object value, DotvvmBindableControl control, DotvvmProperty property);

    }
}