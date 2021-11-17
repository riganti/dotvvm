using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using FastExpressionCompiler;

namespace DotVVM.Framework.Runtime
{
    /// <summary>
    /// Looks for all properties decorated with the <see cref="ParameterBindingAttribute"/> and uses these parameters to bind the values.
    /// </summary>
    public class AttributeViewModelParameterBinder : IViewModelParameterBinder
    {

        private readonly ConcurrentDictionary<Type, Action<IDotvvmRequestContext, object>?> cache = new ConcurrentDictionary<Type, Action<IDotvvmRequestContext, object>?>();
        private readonly MethodInfo setPropertyMethod;

        public AttributeViewModelParameterBinder()
        {
            setPropertyMethod = typeof(AttributeViewModelParameterBinder).GetMethod(nameof(SetProperty), BindingFlags.NonPublic | BindingFlags.Static).NotNull();
        }

        /// <summary>
        /// Performs the parameter binding.
        /// </summary>
        public void BindParameters(IDotvvmRequestContext context, object viewModel)
        {
            var method = cache.GetOrAdd(viewModel.GetType(), BuildParameterBindingMethod);
            method?.Invoke(context, viewModel);
        }

        /// <summary>
        /// Builds a lambda expression which performs the parameter binding for a specified viewmodel type.
        /// </summary>
        private Action<IDotvvmRequestContext, object>? BuildParameterBindingMethod(Type type)
        {
            var properties = FindPropertiesWithParameterBinding(type).ToList();
            if (!properties.Any())
            {
                return null;
            }

            var viewModelParameter = Expression.Parameter(typeof(object), "viewModel");
            var contextParameter = Expression.Parameter(typeof(IDotvvmRequestContext), "context");
            var statements = properties.Select(p => GenerateParameterBindStatement(type, viewModelParameter, contextParameter, p));


            var lambda = Expression.Lambda<Action<IDotvvmRequestContext, object>>(Expression.Block(statements), contextParameter, viewModelParameter);
            return lambda.CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression);
        }

        /// <summary>
        /// Looks up for all properties to be bound.
        /// </summary>
        private Dictionary<PropertyInfo, ParameterBindingAttribute> FindPropertiesWithParameterBinding(Type type)
        {
            return (from prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    let attribute = prop.GetCustomAttribute<ParameterBindingAttribute>()
                    where attribute != null
                    let writableProp = TryFindWritableSetter(prop)
                    where writableProp.CanWrite
                    select new {writableProp, attribute})
                    .ToDictionary(p => p.writableProp, p => p.attribute);
        }

        private PropertyInfo TryFindWritableSetter(PropertyInfo propertyInfo)
        {
            // when the property is declared in the base class and has private set, we need to find the property on the base class to see the SetMethod
            if (propertyInfo.CanWrite)
            {
                return propertyInfo;
            }

            // WORKAROUND: When the type contains non-overridden property with the same name as it's parent (using a `new` modifier) the `GetProperty` method throws an AmbiguousMatchException, so we have to find the property using a collection query
            return propertyInfo.DeclaringType!
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(p => p.Name == propertyInfo.Name && p.DeclaringType == propertyInfo.DeclaringType);
        }

        /// <summary>
        /// Generates an expression which calls the <see cref="SetProperty{T}"/> method to perform the parameter binding.
        /// </summary>
        private Expression GenerateParameterBindStatement(Type viewModelType, Expression viewModelParameter, Expression contextParameter, KeyValuePair<PropertyInfo, ParameterBindingAttribute> property)
        {
            var propAccess = Expression.Property(Expression.Convert(viewModelParameter, viewModelType), property.Key);
            var methodCall = Expression.Call(null, setPropertyMethod.MakeGenericMethod(property.Key.PropertyType), contextParameter, Expression.Constant(property.Value), propAccess);
            return Expression.Assign(propAccess, methodCall);
        }

        /// <summary>
        /// Called from the generated lambda expressions to perform the parameter binding.
        /// </summary>
        private static T? SetProperty<T>(IDotvvmRequestContext context, ParameterBindingAttribute attribute, T? defaultValue)
        {
            if (attribute.TryGetValue<T>(context, out var result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}
