using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractControl : IAbstractContentNode
    {
        IEnumerable<IPropertyDescriptor> PropertyNames { get; }

        bool TryGetProperty(IPropertyDescriptor property, [NotNullWhen(true)] out IAbstractPropertySetter? value);

        object[]? ConstructorParameters { get; set; }
        
    }
}
