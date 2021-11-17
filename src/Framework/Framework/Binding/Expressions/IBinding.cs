using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Binding.Expressions
{
    public enum ErrorHandlingMode
    {
        ReturnNull,
        ThrowException,
        ReturnException
    }
    public interface IBinding
    {
        object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException);

        BindingResolverCollection? GetAdditionalResolvers();
        //IDictionary<Type, object> Properties { get; }
        //IList<Delegate> AdditionalServices { get; }
    }


    public interface ICloneableBinding: IBinding
    {
        IEnumerable<object> GetAllComputedProperties();
    }
}
