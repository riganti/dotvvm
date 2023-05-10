using System;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.AutoUI.PropertyHandlers;

public interface ISelectorDiscoveryService
{
    IValueBinding DiscoverSelectorDataSourceBinding(AutoUIContext autoUiContext, Type propertyType);
}
