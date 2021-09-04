using System;
using System.Linq; 
using System.Collections.Generic;
using DotVVM.Framework.Controls;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.DependencyInjection
{
    [AttributeUsageAttribute(validOn: AttributeTargets.Class, AllowMultiple = false)]
    public class RequireDependencyInjectionAttribute: Attribute
    {
        public readonly Type FactoryType;
        public RequireDependencyInjectionAttribute(Type? factoryType = null)
        {
            if (factoryType != null)
            {
                if (!typeof(Delegate).IsAssignableFrom(factoryType) || !typeof(DotvvmControl).IsAssignableFrom(factoryType.GetMethod("Invoke", new [] { typeof(IServiceProvider), typeof(Type) })?.ReturnType)) throw new ArgumentException($"The specified factory type ({factoryType}) should be a delegate equivalent to Func<IServiceProvider, Type, DotvvmControl>");
                if (factoryType.IsNotPublic)
                    throw new ArgumentException($"The specified factory type ({factoryType}) must be publicly accessible.");
            }
            FactoryType = factoryType ?? typeof(Func<IServiceProvider, Type, DotvvmControl>);
        }
    }
}
