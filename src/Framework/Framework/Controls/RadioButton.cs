using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a HTML radio button.
    /// </summary>
    public class RadioButton : CheckableControlBase
    {
        /// <summary>
        /// Gets or sets whether the control is checked.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty)!; }
            set { SetValue(CheckedProperty, value); }
        }

        public static readonly DotvvmProperty CheckedProperty =
            DotvvmProperty.Register<bool, RadioButton>(t => t.Checked, false);

        /// <summary>
        /// Gets or sets the <see cref="CheckableControlBase.CheckedValue"/> of the first <see cref="RadioButton" /> that is checked and bound to this collection.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public object? CheckedItem
        {
            get { return GetValue(CheckedItemProperty); }
            set { SetValue(CheckedItemProperty, value); }
        }
        public static readonly DotvvmProperty CheckedItemProperty =
            DotvvmProperty.Register<object?, RadioButton>(t => t.CheckedItem, null);

        /// <summary>
        /// Gets or sets an unique name of the radio button group.
        /// </summary>
        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty)!; }
            set { SetValue(GroupNameProperty, value ?? throw new ArgumentNullException(nameof(value))); }
        }
        public static readonly DotvvmProperty GroupNameProperty =
            DotvvmProperty.Register<string, RadioButton>(t => t.GroupName, "");

        protected override void RenderInputTag(IHtmlWriter writer)
        {
            RenderCheckedAttribute(writer);
            RenderCheckedValueAttribute(writer);
            RenderTypeAttribute(writer);
            RenderGroupNameAttribute(writer);

            writer.RenderSelfClosingTag("input");
        }

        protected virtual void RenderGroupNameAttribute(IHtmlWriter writer)
        {
            var valueRaw = GetValueRaw(GroupNameProperty);
            if (valueRaw is IValueBinding valueBinding)
            {
                writer.AddKnockoutDataBind("attr", new KnockoutBindingGroup { { "name", this, valueBinding } });
            }
            var value = KnockoutHelper.TryEvaluateValueBinding(this, valueRaw);
            if (value is not null)
                writer.AddAttribute("name", (string)value);
        }

        protected virtual void RenderTypeAttribute(IHtmlWriter writer)
        {
            // render the input tag
            writer.AddAttribute("type", "radio");
        }

        protected virtual void RenderCheckedValueAttribute(IHtmlWriter writer)
        {
            var checkedValue = GetValueOrBinding<object?>(CheckedValueProperty);
            if (!checkedValue.ValueIsNull())
            {
                var checkedValueExpr = checkedValue.GetJsExpression(this);
                writer.AddKnockoutDataBind("checkedValue", checkedValueExpr);
            }
            RenderCheckedValueComparerAttribute(writer);
        }

        protected virtual void RenderCheckedAttribute(IHtmlWriter writer)
        {
            var checkedItemBinding = GetValueBinding(CheckedItemProperty);
            if (checkedItemBinding is null)
            {
                var @checked = GetValueRaw(CheckedProperty);
                if (@checked is IValueBinding checkedBinding)
                {
                    writer.AddKnockoutDataBind("checked", this, checkedBinding);
                }
                if (true.Equals(KnockoutHelper.TryEvaluateValueBinding(this, @checked)))
                {
                    writer.AddAttribute("checked", "");
                }

                // if (!IsPropertySet(CheckedValueProperty))
                // {
                //     throw new DotvvmControlException(this, "The 'CheckedValue' of the RadioButton control must be set. Remember that all RadioButtons with the same GroupName should be bound to the same property in the viewmodel.");
                // }
            }
            else
            {
                // selected item mode
                writer.AddKnockoutDataBind("checked", checkedItemBinding.GetKnockoutBindingExpression(this));
            }
        }

        [ControlUsageValidator]
        public static new IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            var to = control.GetValue(CheckedItemProperty)?.GetResultType();
            var nonNullableTo = to?.UnwrapNullableType();
            var from = control.GetValue(CheckedValueProperty)?.GetResultType();

            if (to != null && from != null
                && !to.IsAssignableFrom(from) && !nonNullableTo!.IsAssignableFrom(from))
            {
                yield return new ControlUsageError(
                    $"CheckedItem type \'{to}\' must be the same as or a nullable " +
                    $"variant of the CheckedValue type \'{from}\'.",
                    control.GetValue(CheckedItemProperty)?.DothtmlNode,
                    control.GetValue(CheckedValueProperty)?.DothtmlNode
                );
            }
        }
    }
}
