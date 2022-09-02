using DotVVM.Framework.Utils;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Base class for control that allows to select one of its items.
    /// </summary>
    public abstract class Selector : SelectorBase
    {
        protected Selector(string tagName)
            : base(tagName)
        {
        }

        /// <summary>
        /// Gets or sets the value of the selected item.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public object? SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }
        public static readonly DotvvmProperty SelectedValueProperty =
            DotvvmProperty.Register<object?, Selector>(t => t.SelectedValue);

        [ControlUsageValidator]
        public static new IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            if (!(control.GetValue(SelectedValueProperty) is ResolvedPropertySetter selectedValueBinding)) yield break;
            if (control.GetValue(ItemValueBindingProperty) is ResolvedPropertySetter itemValueBinding)
            {
                var to = selectedValueBinding.GetResultType();
                var from = itemValueBinding.GetResultType();

                if (!IsValueAssignable(from, to))
                {
                    yield return CreateSelectedValueTypeError(selectedValueBinding, to, from);
                }
            }
            else if (control.GetValue(DataSourceProperty) is ResolvedPropertySetter dataSourceBinding)
            {
                var to = selectedValueBinding.GetResultType();
                var from = dataSourceBinding.GetResultType()?.UnwrapNullableType()?.GetEnumerableType();

                if (!IsDataSourceItemAssignable(from, to))
                {
                    yield return CreateSelectedValueTypeError(selectedValueBinding, to, from);
                }
            }
        }

        protected static ControlUsageError CreateSelectedValueTypeError(ResolvedPropertySetter selectedValueBinding, Type? to, Type? from)
            => new ($"Type '{from?.FullName ?? "?"}' is not assignable to '{to?.FullName ?? "?"}'.", selectedValueBinding.DothtmlNode);

        protected static bool IsValueAssignable(Type? valueType, Type? to)
        {
            var nonNullableTo = to?.UnwrapNullableType();

            return
                to == null ||
                valueType == null ||
                to.IsAssignableFrom(valueType) ||
                nonNullableTo!.IsAssignableFrom(valueType);
        }

        protected static bool IsDataSourceItemAssignable(Type? itemType, Type? to)
        {
            var nonNullableTo = to?.UnwrapNullableType();

            return
                to == null ||
                itemType == null ||
                to.IsAssignableFrom(itemType) ||
                nonNullableTo!.IsAssignableFrom(itemType) ||
                (to.IsEnum && itemType == typeof(string)) ||
                (to.UnwrapNullableType().IsEnum && itemType == typeof(string));
        }
    }
}
