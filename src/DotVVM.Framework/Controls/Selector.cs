using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }
        public static readonly DotvvmProperty SelectedValueProperty =
            DotvvmProperty.Register<object, Selector>(t => t.SelectedValue);

        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            var collectionType = control.GetValue(DataSourceProperty)?.GetResultType().UnwrapNullableType();
            var itemKeyType = control.GetValue(ItemValueBindingProperty)?.GetResultType();
            var valueType = control.GetValue(SelectedValueProperty)?.GetResultType();
            var collectionValueType = itemKeyType ?? collectionType?.Apply(ReflectionUtils.GetEnumerableType);

            if (collectionValueType != null && valueType != null && valueType != collectionValueType && valueType.UnwrapNullableType() != collectionValueType)
            {
                yield return new ControlUsageError(
                    $"Type of items in {(itemKeyType == null ? "DataSource" : "ItemKeyBinding")} \'{collectionValueType}\' must be same as SelectedValue type \'{valueType}\'.",
                    control.GetValue(SelectedValueProperty).DothtmlNode,
                    control.GetValue(DataSourceProperty).DothtmlNode
                );
            }
        }
    }
}
