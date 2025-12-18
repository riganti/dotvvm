using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Binding.Expressions
{
    /// <summary> Controls what happens when the binding property does not exist on this binding or when its resolver throws an exception. </summary>
    public enum ErrorHandlingMode
    {
        /// <summary> Returns null. The null is returned even in case when resolver throws an exception, you can't distinguish between the "property does not exist", "resolver failed" states using this mode. </summary>
        ReturnNull,
        /// <summary> Throws the exception. Always throws <see cref="BindingPropertyException" />. In case the property is missing, message = "resolver not found". Otherwise, the exception will have the resolver error as InnerException. </summary>
        ThrowException,
        /// <summary> Behaves similarly to ThrowException, but the exception is returned instead of being thrown. This is useful when you'd catch the exception immediately to avoid annoying debugger by throwing too many exceptions. </summary>
        ReturnException
    }

    /// <summary> General interface which all DotVVM data binding types must implement. This interface does not provide any specific binding properties, only the basic building blocks - that bindings are composed of binding properties (<see cref="GetProperty(Type, ErrorHandlingMode)" />), should have a DataContext and may have resolvers. </summary>
    public interface IBinding
    {
        /// <summary> Gets the binding property identified by the type. Returned object will always be of type <paramref name="type"/>, null, or Exception (this depends on the <paramref name="errorMode" />). This method should always return the same result and should run fast (may rely on caching, so first call might not be that fast). </summary>
        object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException);

        /// <summary> Gets the binding property identified by the type, or throws an exception. This method should always return the same result and should run fast (may rely on caching, so first call might not be that fast). </summary>
        T GetProperty<T>();

        /// <summary> Gets the binding property identified by the type. This method should always return the same result and should run fast (may rely on caching, so first call might not be that fast). </summary>
        bool TryGetPropety<T>([NotNullWhen(true)] out T? value);

        /// <summary> Gets the binding property identified by the type. This method should always return the same result and should run fast (may rely on caching, so first call might not be that fast). </summary>
        bool TryGetPropety<T>([NotNullWhen(true)] out T? value, [NotNullWhen(false)] out Exception? error);

        /// <summary> If the binding expects a specific data context, this property should return it. "Normal" binding coming from dothtml markup won't return null since they always depend on the data context. </summary>
        DataContextStack? DataContext { get; }

        BindingResolverCollection? GetAdditionalResolvers();
    }


    public interface ICloneableBinding: IBinding
    {
        /// <summary> Returns a list of all properties which are already cached. Creating a new binding with these properties will produce the same binding. </summary>
        IEnumerable<object> GetAllComputedProperties();
    }
}
