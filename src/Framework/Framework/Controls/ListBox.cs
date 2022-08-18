using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the HTML list box.
    /// </summary>
    public class ListBox : SelectHtmlControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        public ListBox()
        {
            
        }

        /// <summary>
        /// Gets or sets number of rows visible in this ListBox.
        /// </summary>
        public int Size
        {
            get { return (int)GetValue(SizeProperty)!; }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DotvvmProperty SizeProperty =
            DotvvmProperty.Register<int, ListBox>(t => t.Size, defaultValue: 10);

        /// <summary>
        /// Gets or sets if lisbox can be used to select single item, or multiple items at once
        /// Does not support binding
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public ListBoxSelectionMode SelectionMode
        {
            get { return (ListBoxSelectionMode?)GetValue(SelectionModeProperty) ?? ListBoxSelectionMode.Single; }
            set { SetValue(SelectionModeProperty, value); }
        }
        public static readonly DotvvmProperty SelectionModeProperty
            = DotvvmProperty.Register<ListBoxSelectionMode, ListBox>(c => c.SelectionMode, ListBoxSelectionMode.Single);

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.AddAttributesToRender(writer, context);
            writer.AddKnockoutDataBind("size", this, SizeProperty, () => writer.AddAttribute("size", Size.ToString()));
        }

        protected override void RenderSelectedValueProperty(IHtmlWriter writer)
        {
            if (SelectionMode == ListBoxSelectionMode.Multiple)
            {
                writer.AddAttribute("multiple", "multiple");
                writer.AddKnockoutDataBind("selectedOptions", this, SelectedValueProperty, renderEvenInServerRenderingMode: true);
            }
            else
            {
                writer.AddKnockoutDataBind("value", this, SelectedValueProperty, renderEvenInServerRenderingMode: true);
            }
            writer.AddKnockoutDataBind("valueAllowUnset", "true");
        }

        [ControlUsageValidator(Override = true)]
        public static new IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            if (!(control.GetValue(SelectedValueProperty) is ResolvedPropertySetter selectedValueBinding)) yield break;

            var selectionMode = (ListBoxSelectionMode?)control.GetValue(SelectionModeProperty)?.GetValue() ?? ListBoxSelectionMode.Single;

            if (control.GetValue(ItemValueBindingProperty) is ResolvedPropertySetter itemValueBinding)
            {
                var to = GetSelectedValueType(selectedValueBinding, selectionMode);
                var from = itemValueBinding.GetResultType();

                if (!IsValueAssignable(from, to))
                {
                    yield return CreateSelectedValueTypeError(selectedValueBinding, to, from);
                }
            }
            else if (control.GetValue(DataSourceProperty) is ResolvedPropertySetter dataSourceBinding)
            {
                var to = GetSelectedValueType(selectedValueBinding, selectionMode);
                var from = dataSourceBinding.GetResultType()?.UnwrapNullableType()?.GetEnumerableType();

                if (!IsDataSourceItemAssignable(from, to))
                {
                    yield return CreateSelectedValueTypeError(selectedValueBinding, to, from);
                }
            }
        }

        protected static Type? GetSelectedValueType(ResolvedPropertySetter selectedValueBinding, ListBoxSelectionMode selectionMode)
        {
            return selectionMode == ListBoxSelectionMode.Multiple
                ? selectedValueBinding.GetResultType()?.UnwrapNullableType()?.GetEnumerableType()
                : selectedValueBinding.GetResultType();
        }
    }
}
