using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Binding;
using FastExpressionCompiler;

namespace DotVVM.Framework.Binding
{
    public static class BindingFactory
    {
        private static ConcurrentDictionary<Type, Func<BindingCompilationService, object?[], IBinding>> bindingCtorCache = new ConcurrentDictionary<Type, Func<BindingCompilationService, object?[], IBinding>>();

        /// <summary>
        /// Creates the binding by calling .ctor(BindingCompilationService service, object[] properties), does not wrap exceptions to TargetInvocationException.
        /// </summary>
        public static IBinding CreateBinding(this BindingCompilationService service, Type binding, object?[] properties)
        {
            if (binding.ContainsGenericParameters)
            {
                var type = properties.OfType<ExpectedTypeBindingProperty>().FirstOrDefault()?.Type;
                if (type is null)
                {
                    var nonGenericBase = binding.BaseType;
                    Debug.Assert(nonGenericBase != null && !nonGenericBase.ContainsGenericParameters && nonGenericBase.Name  + "`1" == binding.Name);
                    var tmpBinding = CreateBinding(service, nonGenericBase!, properties);
                    type = tmpBinding.GetProperty<ExpectedTypeBindingProperty>(ErrorHandlingMode.ReturnNull)?.Type ??
                            tmpBinding.GetProperty<ResultTypeBindingProperty>().Type;
                    if (tmpBinding is ICloneableBinding cloneable)
                        properties = cloneable.GetAllComputedProperties().ToArray();
                }
                binding = binding.MakeGenericType(new [] { type });
            }

            return bindingCtorCache.GetOrAdd(binding, static type => {
                var ctor = type.GetConstructor(new[] { typeof(BindingCompilationService), typeof(object[]) }) ??
                           type.GetConstructor(new[] { typeof(BindingCompilationService), typeof(IEnumerable<object>) });
                if (ctor == null) throw new NotSupportedException($"Could not find .ctor(BindingCompilationService service, object[] properties) on binding '{type.FullName}'.");
                var bindingServiceParam = Expression.Parameter(typeof(BindingCompilationService));
                var propertiesParam = Expression.Parameter(typeof(object?[]));
                var expression = Expression.New(ctor, bindingServiceParam, TypeConversion.EnsureImplicitConversion(propertiesParam, ctor.GetParameters()[1].ParameterType));
                return Expression.Lambda<Func<BindingCompilationService, object?[], IBinding>>(expression, bindingServiceParam, propertiesParam).CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression);
            })(service, properties);
        }
    }
}
