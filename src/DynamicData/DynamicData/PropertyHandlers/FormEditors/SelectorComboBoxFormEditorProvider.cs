using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Controls.DynamicData.ViewModel;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    public class SelectorComboBoxFormEditorProvider : FormEditorProviderBase
    {
        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            return context.PropertyDisplayMetadataProvider.GetPropertyMetadata(propertyInfo).SelectorConfiguration != null;
        }

        public override DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicEditor.Props props, DynamicDataContext context)
        {
            var selectorConfiguration = property.SelectorConfiguration!;
            var selectorDataSourceBinding = DiscoverSelectorDataSourceBinding(context, selectorConfiguration.PropertyType);

            return new ComboBox()
                .SetCapability(props.Html)
                .SetProperty(c => c.DataSource, selectorDataSourceBinding)
                .SetProperty(c => c.ItemTextBinding, context.CreateValueBinding("DisplayName", selectorConfiguration.PropertyType))
                .SetProperty(c => c.ItemValueBinding, context.CreateValueBinding("Id", selectorConfiguration.PropertyType))
                .SetProperty(c => c.SelectedValue, props.Property)
                .SetProperty(c => c.Enabled, props.Enabled)
                .SetProperty(c => c.SelectionChanged, props.Changed);
        }

        private IValueBinding DiscoverSelectorDataSourceBinding(DynamicDataContext dynamicDataContext, Type propertyType)
        {
            var viewModelType = typeof(ISelectorViewModel<>).MakeGenericType(propertyType);

            var parentIndex = 1;
            foreach (var parent in dynamicDataContext.DataContextStack.Parents())
            {
                var matchingProperties = parent
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.CanRead)
                    .Where(p => viewModelType.IsAssignableFrom(p.PropertyType))
                    .ToArray();
                if (matchingProperties.Length > 1)
                {
                    throw new DotvvmControlException($"More than one property of type {viewModelType.FullName} was found in {parent.FullName} viewmodel!");
                }
                else if (matchingProperties.Length == 1)
                {
                    var param = Expression.Parameter(typeof(object[]));
                    var body =
                        Expression.Property(
                            Expression.Property(
                                Expression.Convert(
                                    Expression.ArrayIndex(param, Expression.Constant(parentIndex)),
                                    parent
                                ),
                                matchingProperties[0]
                            ),
                            nameof(ISelectorViewModel<Annotations.SelectorItem>.Items)
                        );
                    return ValueBindingExpression.CreateBinding(
                        dynamicDataContext.BindingService,
                        Expression.Lambda<Func<object?[], object>>(body, param),
                        dynamicDataContext.DataContextStack);
                }

                parentIndex++;
            }

            throw new DotvvmControlException($"No property of type {viewModelType.FullName} was found in the viewmodel {dynamicDataContext.DataContextStack}!");
        }
    }
}
