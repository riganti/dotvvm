using DotVVM.Framework.Utils;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
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
        public static new IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            if (!(control.Properties.GetValueOrDefault(Selector.SelectedValueProperty) is ResolvedPropertyBinding selectedValueBinding)) yield break;
            if (control.Properties.GetValueOrDefault(SelectorBase.ItemValueBindingProperty) is ResolvedPropertyBinding itemValueBinding)
            {
                var to = selectedValueBinding.Binding.ResultType;
                var from = itemValueBinding.Binding.ResultType;
                if (!from.IsAssignableTo(to))
                {
                    yield return new ControlUsageError($"Type '{from.FullName}' is not assignable to '{to.FullName}'.", selectedValueBinding.Binding.DothtmlNode);
                }
            }
            else
            {
                if (control.Properties.GetValueOrDefault(ItemsControl.DataSourceProperty) is ResolvedPropertyBinding dataSourceBinding)
                {
                    var to = selectedValueBinding.Binding.ResultType;
                    var from = dataSourceBinding.Binding.ResultType.TryGetArrayElementOrIEnumerableType();
                    if (!from.IsAssignableTo(to) && !(to.IsEnumTypeDescriptor() && from.IsStringTypeDescriptor()))
                    {
                        yield return new ControlUsageError($"Type '{from.FullName}' is not assignable to '{to.FullName}'.", selectedValueBinding.Binding.DothtmlNode);
                    }
                }
            }
        }
    }
}
