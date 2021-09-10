using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Binding
{
    public abstract class BindingCompilationOptionsAttribute : Attribute
    {
        public abstract IEnumerable<Delegate> GetResolvers();
    }
}
