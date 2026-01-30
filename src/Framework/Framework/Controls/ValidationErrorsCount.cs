using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationErrorsCount"/> class.
        /// </summary>
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

        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            TagName = WrapperTagName;
            base.OnPreRender(context);
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
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
            }
            writer.AddKnockoutDataBind("dotvvm-validationErrorsCount", group);

            Validator.AddValidationOptionsBinding(writer, this);
        }
    }
}
