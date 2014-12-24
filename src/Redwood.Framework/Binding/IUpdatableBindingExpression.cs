using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Binding
{
    public interface IUpdatableBindingExpression
    {

        void UpdateSource(object value, RedwoodBindableControl control, RedwoodProperty property);

    }
}