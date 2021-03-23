#nullable enable
using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A base control for checkbox and radiobutton controls.
    /// </summary>
    public abstract class CheckableControlBase : HtmlGenericControl
    {
        private bool isLabelRequired;

        /// <summary>
        /// Gets or sets the label text that is rendered next to the control.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty)!; }
            set { SetValue(TextProperty, value ?? throw new ArgumentNullException(nameof(value))); }
        }

        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<string, CheckableControlBase>(t => t.Text, "");

        /// <summary>
        /// Gets or sets the value that will be used as a result when the control is checked.
        /// Use this property in combination with the CheckedItem or CheckedItems property.
        /// </summary>
        public object? CheckedValue
        {
            get { return GetValue(CheckedValueProperty); }
            set { SetValue(CheckedValueProperty, value); }
        }

        public static readonly DotvvmProperty CheckedValueProperty =
            DotvvmProperty.Register<object?, CheckableControlBase>(t => t.CheckedValue, null);

        /// <summary>
        /// Gets or sets the command that will be triggered when the control check state is changed.
        /// </summary>
        public Command? Changed
        {
            get { return (Command?)GetValue(ChangedProperty); }
            set { SetValue(ChangedProperty, value); }
        }

        public static readonly DotvvmProperty ChangedProperty =
            DotvvmProperty.Register<Command?, CheckableControlBase>(t => t.Changed, null);

        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled and can be clicked on.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty)!; }
            set { SetValue(EnabledProperty, value); }
        }

        public static readonly DotvvmProperty EnabledProperty =
            DotvvmPropertyWithFallback.Register<bool, CheckableControlBase>(nameof(Enabled), FormControls.EnabledProperty);


        /// <summary>
        /// Gets or sets a property that retrieves an unique key for the CheckedValue so it can be compared with objects in the CheckedItems collection. This property must be set if the value of the CheckedValue property is not a primitive type.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        [ControlPropertyBindingDataContextChange(nameof(CheckedValue))]
        [BindingCompilationRequirements(required: new[] { typeof(SelectorItemBindingProperty) })]
        public IValueBinding? ItemKeyBinding
        {
            get { return (IValueBinding?)GetValue(ItemKeyBindingProperty); }
            set { SetValue(ItemKeyBindingProperty, value); }
        }
        public static readonly DotvvmProperty ItemKeyBindingProperty
            = DotvvmProperty.Register<IValueBinding, CheckableControlBase>(c => c.ItemKeyBinding);



        /// <summary>
        /// Initializes a new instance of the <see cref="CheckableControlBase"/> class.
        /// </summary>
        public CheckableControlBase() : base("span")
        {

        }

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            base.OnPreRender(context);

            isLabelRequired = HasValueBinding(TextProperty) || !string.IsNullOrEmpty(Text) || !HasOnlyWhiteSpaceContent();
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (isLabelRequired)
            {
                writer.RenderBeginTag("label");
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // label end tag
            if (isLabelRequired)
            {
                writer.RenderEndTag();
            }
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (TagName is null) throw new DotvvmControlException(this, "CheckableControlBase must have a tag name");
            AddAttributesToInput(writer);
            RenderInputTag(writer);

            if (isLabelRequired)
            {
                if (GetValueBinding(TextProperty) is IValueBinding textBinding)
                {
                    writer.AddKnockoutDataBind("text", textBinding.GetKnockoutBindingExpression(this));
                    writer.RenderBeginTag(TagName);
                    writer.RenderEndTag();
                }
                else if (!string.IsNullOrEmpty(Text))
                {
                    writer.RenderBeginTag(TagName);
                    writer.WriteText(Text);
                    writer.RenderEndTag();
                }
                else if (!HasOnlyWhiteSpaceContent())
                {
                    writer.RenderBeginTag(TagName);
                    RenderChildren(writer, context);
                    writer.RenderEndTag();
                }
            }
        }

        protected virtual void AddAttributesToInput(IHtmlWriter writer)
        {
            // prepare changed event attribute
            var changedBinding = GetCommandBinding(ChangedProperty);
            if (changedBinding != null)
            {
                writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(nameof(Changed), changedBinding, this, useWindowSetTimeout: true, returnValue: true, isOnChange: true));
            }

            // handle enabled attribute
            writer.AddKnockoutDataBind("enable", this, EnabledProperty, () =>
            {
                if (!Enabled)
                {
                    writer.AddAttribute("disabled", "disabled");
                }
            });
        }

        protected virtual void RenderCheckedValueComparerAttribute(IHtmlWriter writer)
        {
            if (ItemKeyBinding != null)
            {
                writer.AddKnockoutDataBind("checkedValueComparer",
                    ItemKeyBinding.GetProperty<SelectorItemBindingProperty>().Expression.KnockoutExpression.FormatKnockoutScript(this, GetBinding(CheckedValueProperty)!));
            }
        }

        /// <summary>
        /// Renders the input tag.
        /// </summary>
        protected abstract void RenderInputTag(IHtmlWriter writer);


        [ControlUsageValidator]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            var keySelector = control.GetValue(ItemKeyBindingProperty)?.GetResultType();
            if (keySelector != null)
            {
                if (!ReflectionUtils.IsPrimitiveType(keySelector))
                {
                    yield return new ControlUsageError("The ItemKeyBinding property must return a value of a primitive type.");
                }
                else if (!(control.GetValue(CheckedValueProperty) is ResolvedPropertyBinding))
                {
                    yield return new ControlUsageError("The ItemKeyBinding property can be only used when CheckedValue is a binding.");
                }
            }

            var from = control.GetValue(CheckedValueProperty)?.GetResultType();
            if (keySelector == null && from != null && !ReflectionUtils.IsPrimitiveType(from))
            {
                yield return new ControlUsageError("The ItemKeyBinding property must be specified when the CheckedValue property contains a complex type.");
            }
        }
    }
}
