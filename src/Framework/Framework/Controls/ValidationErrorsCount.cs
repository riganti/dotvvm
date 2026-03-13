using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Displays the count of validation errors from the current Validation.Target.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class ValidationErrorsCount : HtmlGenericControl
    {
        public ValidationErrorsCount()
            : base("span", false)
        {
        }

        /// <summary>
        /// Gets or sets whether the errors from child objects in the viewmodel will be counted too. 
        /// Default is <c>true</c> (unlike <see cref="ValidationSummary"/> which defaults to <c>false</c>).
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool IncludeErrorsFromChildren
        {
            get { return (bool)GetValue(IncludeErrorsFromChildrenProperty)!; }
            set { SetValue(IncludeErrorsFromChildrenProperty, value); }
        }
        public static readonly DotvvmProperty IncludeErrorsFromChildrenProperty
            = DotvvmProperty.Register<bool, ValidationErrorsCount>(c => c.IncludeErrorsFromChildren, true);

        /// <summary>
        /// Gets or sets whether the errors from the <see cref="Validation.TargetProperty"/> object will be counted too.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool IncludeErrorsFromTarget
        {
            get { return (bool)GetValue(IncludeErrorsFromTargetProperty)!; }
            set { SetValue(IncludeErrorsFromTargetProperty, value); }
        }
        public static readonly DotvvmProperty IncludeErrorsFromTargetProperty
            = DotvvmProperty.Register<bool, ValidationErrorsCount>(c => c.IncludeErrorsFromTarget, true);

        /// <summary>
        /// Gets or sets the name of the tag that wraps the control.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string WrapperTagName
        {
            get { return (string)GetValue(WrapperTagNameProperty)!; }
            set { SetValue(WrapperTagNameProperty, value); }
        }
        public static readonly DotvvmProperty WrapperTagNameProperty
            = DotvvmProperty.Register<string, ValidationErrorsCount>(c => c.WrapperTagName, "span");

        /// <summary>
        /// Gets or sets the CSS class that will be applied to the control when there are validation errors.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string? InvalidCssClass
        {
            get { return (string?)GetValue(InvalidCssClassProperty); }
            set { SetValue(InvalidCssClassProperty, value); }
        }
        public static readonly DotvvmProperty InvalidCssClassProperty
            = DotvvmProperty.Register<string?, ValidationErrorsCount>(c => c.InvalidCssClass, null);

        /// <summary>
        /// Gets or sets whether the control will be hidden when there are no validation errors. Default is <c>false</c>.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool HideWhenValid
        {
            get { return (bool)GetValue(HideWhenValidProperty)!; }
            set { SetValue(HideWhenValidProperty, value); }
        }
        public static readonly DotvvmProperty HideWhenValidProperty
            = DotvvmProperty.Register<bool, ValidationErrorsCount>(c => c.HideWhenValid, false);

        /// <summary>
        /// Gets or sets a function that formats the error count. The function receives the error count as a parameter and should return the string to be displayed. If not set, the error count will be displayed as a string.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public IValueBinding<Func<int, string>>? FormatErrorsCount
        {
            get { return (IValueBinding<Func<int, string>>?)GetValue(FormatErrorsCountProperty); }
            set { SetValue(FormatErrorsCountProperty, value); }
        }
        public static readonly DotvvmProperty FormatErrorsCountProperty
            = DotvvmProperty.Register<IValueBinding<Func<int, string>>?, ValidationErrorsCount>(c => c.FormatErrorsCount, null);


        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            TagName = WrapperTagName;
            base.OnPreRender(context);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.AddAttributesToRender(writer, context);

            if (false.Equals(this.GetValue(Validation.EnabledProperty)))
            {
                return;
            }

            var expression = this.GetValueBinding(Validation.TargetProperty)?.GetKnockoutBindingExpression(this) ?? "dotvvm.viewModelObservables.root";

            var group = new KnockoutBindingGroup();
            {
                group.Add("target", expression);
                group.Add("includeErrorsFromChildren", IncludeErrorsFromChildren.ToString().ToLowerInvariant());
                group.Add("includeErrorsFromTarget", IncludeErrorsFromTarget.ToString().ToLowerInvariant());
                if (!string.IsNullOrEmpty(InvalidCssClass))
                {
                    group.Add("invalidCssClass", this, InvalidCssClassProperty);
                }
                if (HideWhenValid)
                {
                    group.Add("hideWhenValid", this, HideWhenValidProperty);
                }
                if (HasBinding(FormatErrorsCountProperty))
                {
                    group.Add("formatErrorsCount", this, FormatErrorsCountProperty);
                }
            }
            writer.AddKnockoutDataBind("dotvvm-validationErrorsCount", group);
        }
    }
}
