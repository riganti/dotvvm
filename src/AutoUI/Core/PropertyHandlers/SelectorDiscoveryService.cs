using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.AutoUI.ViewModel;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Controls;

namespace DotVVM.AutoUI.PropertyHandlers
{
    public class SelectorDiscoveryService : ISelectorDiscoveryService 
    {

        public IValueBinding DiscoverSelectorDataSourceBinding(AutoUIContext autoUiContext, Type propertyType)
        {
            var viewModelType = typeof(ISelectorViewModel<>).MakeGenericType(propertyType);

            var dataContextStack = autoUiContext.DataContextStack;
            while (dataContextStack != null)
            {
                var param = Expression.Parameter(dataContextStack.DataContextType, "p").AddParameterAnnotation(new BindingParameterAnnotation(dataContextStack));

                var matchingProperties = FindSelectorProperties(param, viewModelType);
                if (matchingProperties.Length > 1)
                {
                    throw new DotvvmControlException($"More than one property of type {viewModelType.FullName} was found in {dataContextStack.DataContextType.FullName} viewmodel!");
                }
                else if (matchingProperties.Length == 1)
                {
                    var body =
                        Expression.Property(
                            matchingProperties[0],
                            nameof(ISelectorViewModel<Annotations.Selection>.Items)
                        );
                    return (IValueBinding)autoUiContext.BindingService.CreateBinding(
                        typeof(ValueBindingExpression<>),
                        new object[]
                        {
                            new ParsedExpressionBindingProperty(body),
                            autoUiContext.DataContextStack
                        });
                }

                dataContextStack = dataContextStack.Parent;
            }

            throw new DotvvmControlException($"No property of type {viewModelType.FullName} was found in the viewmodel {autoUiContext.DataContextStack}!");
        }

        protected virtual Expression[] FindSelectorProperties(Expression parent, Type selectorViewModelType)
        {
            var directProperties = parent.Type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .Where(p => selectorViewModelType.IsAssignableFrom(p.PropertyType))
                .Select(p => Expression.Property(parent, p));

            var tupleProperties = parent.Type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .Where(p => p.PropertyType.FullName!.StartsWith("System.Tuple`"))
                .SelectMany(p => p.PropertyType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(i => selectorViewModelType.IsAssignableFrom(i.PropertyType))
                    .Select(i => new { Tuple = p, Property = i }))
                .Select(p => Expression.Property(Expression.Property(parent, p.Tuple), p.Property));

            return directProperties.Concat(tupleProperties).ToArray();
        }
    }
}
