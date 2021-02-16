#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.Validation;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the HTML checkbox control.
    /// </summary>
    public class CheckBox : CheckableControlBase
    {
        /// <summary>
        /// Gets or sets whether the control is checked.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public bool? Checked
        {
            get { return (bool?)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        public static readonly DotvvmProperty CheckedProperty =
            DotvvmProperty.Register<bool?, CheckBox>(t => t.Checked, false);

        /// <summary>
        /// Gets or sets a collection of values of all checked checkboxes. Use this property in combination with the CheckedValue property.
        /// </summary>
        public IEnumerable? CheckedItems
        {
            get { return (IEnumerable?)GetValue(CheckedItemsProperty); }
            set { SetValue(CheckedItemsProperty, value); }
        }
        public static readonly DotvvmProperty CheckedItemsProperty =
            DotvvmProperty.Register<IEnumerable?, CheckBox>(t => t.CheckedItems, null);


        /// <summary>
        /// Renders the input tag.
        /// </summary>
        protected override void RenderInputTag(IHtmlWriter writer)
        {
            if (HasValueBinding(CheckedProperty) && !HasValueBinding(CheckedItemsProperty))
            {
                // boolean mode
                RenderCheckedProperty(writer);
            }
            else if (!HasValueBinding(CheckedProperty) && HasValueBinding(CheckedItemsProperty))
            {
                // collection mode
                RenderCheckedItemsProperty(writer);
            }
            else
            {
                throw new DotvvmControlException(this, "Either the Checked or the CheckedItems binding of a CheckBox must be set.");
            }

            RenderTypeAttribute(writer);
            writer.RenderSelfClosingTag("input");
        }

        protected virtual void RenderTypeAttribute(IHtmlWriter writer)
        {
            // render the input tag
            writer.AddAttribute("type", "checkbox");
        }

        protected virtual void RenderCheckedItemsProperty(IHtmlWriter writer)
        {
            RenderCheckedItemsBinding(writer);
            writer.AddKnockoutDataBind("checkedArrayContainsObservables", "true");
            writer.AddKnockoutDataBind("dotvvm-checkbox-updateAfterPostback", "true");
            RenderDotvvmCheckedPointerBinding(writer);
            writer.AddKnockoutDataBind("checkedValue", this, CheckedValueProperty, () => {
                var checkedValue = (CheckedValue ?? string.Empty).ToString();
                if (!string.IsNullOrEmpty(checkedValue))
                {
                    writer.AddKnockoutDataBind("checkedValue", KnockoutHelper.MakeStringLiteral(checkedValue));
                }
            });
            RenderCheckedValueComparerAttribute(writer);
        }

        protected virtual void RenderDotvvmCheckedPointerBinding(IHtmlWriter writer)
        {
            writer.AddKnockoutDataBind("dotvvm-checked-pointer", GetDotvvmCheckedPointerBindingValue());
        }

        protected virtual string GetDotvvmCheckedPointerBindingValue()
        {
            if (HasValueBinding(CheckedItemsProperty))
            {
                return "'dotvvm-checkedItems'";
            }
            return "'checked'";
        }

        protected virtual void RenderCheckedItemsBinding(IHtmlWriter writer)
        {
            var checkedItemsBinding = GetValueBinding(CheckedItemsProperty);
            writer.AddKnockoutDataBind("dotvvm-checkedItems", checkedItemsBinding!.GetKnockoutBindingExpression(this));
        }

        protected virtual void RenderCheckedProperty(IHtmlWriter writer)
        {
            var checkedBinding = GetValueBinding(CheckedProperty);
            writer.AddKnockoutDataBind("dotvvm-CheckState", checkedBinding!, this);

            // Boolean mode can have prerendered `checked` attribute
            if (RenderOnServer && true.Equals(GetValue(CheckedProperty)))
                writer.AddAttribute("checked", null);
        }


        [ControlUsageValidator]
        public new static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            var to = control.GetValue(CheckedItemsProperty)?.GetResultType()?.UnwrapNullableType()?.GetEnumerableType();
            var nonNullableTo = to?.UnwrapNullableType();
            var from = control.GetValue(CheckedValueProperty)?.GetResultType();

            if (to != null && from != null
                && !to.IsAssignableFrom(from) && !nonNullableTo!.IsAssignableFrom(from))
            {
                yield return new ControlUsageError(
                    $"Type of items in CheckedItems \'{to}\' must be same as CheckedValue type \'{from}\'.",
                    control.GetValue(CheckedItemsProperty).DothtmlNode,
                    control.GetValue(CheckedValueProperty).DothtmlNode
                );
            }
        }

    }
}
