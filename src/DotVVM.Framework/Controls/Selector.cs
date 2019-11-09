#nullable enable
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
                var nonNullableTo = to?.UnwrapNullableType();
                var from = itemValueBinding.GetResultType();

                if (to != null && from != null
                    && !to.IsAssignableFrom(from) && !nonNullableTo!.IsAssignableFrom(from))
                {
                    yield return new ControlUsageError($"Type '{from.FullName}' is not assignable to '{to.FullName}'.", selectedValueBinding.DothtmlNode);
                }
            }
            else if (control.GetValue(DataSourceProperty) is ResolvedPropertySetter dataSourceBinding)
            {
                var to = selectedValueBinding.GetResultType();
                var nonNullableTo = to?.UnwrapNullableType();
                var from = dataSourceBinding.GetResultType()?.UnwrapNullableType()?.GetEnumerableType();

                if (to != null && from != null
                    && !to.IsAssignableFrom(from) && !nonNullableTo!.IsAssignableFrom(from)
                    && !(to.IsEnum && from == typeof(string)) && !(to.UnwrapNullableType().IsEnum && from == typeof(string)))
                {
                    yield return new ControlUsageError($"Type '{from.FullName}' is not assignable to '{to.FullName}'.", selectedValueBinding.DothtmlNode);
                }
            }
        }
    }
}
