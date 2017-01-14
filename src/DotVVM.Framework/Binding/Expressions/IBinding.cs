using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Binding.Expressions
{
    public interface IBinding
    {
        object GetProperty(Type type, bool optional = false);
        //IDictionary<Type, object> Properties { get; }
        //IList<Delegate> AdditionalServices { get; }
    }

    public interface IMutableBinding: IBinding
    {
        void AddProperty(object property);
        bool IsMutable { get; }
    }
}
