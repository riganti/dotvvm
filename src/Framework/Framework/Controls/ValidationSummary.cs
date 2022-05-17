using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Displays all validation messages from the current Validation.Target.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class ValidationSummary : HtmlGenericControl
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationSummary"/> class.
        /// </summary>
        public ValidationSummary()
        {
            TagName = "ul";
        }


        /// <summary>
        /// Gets or sets whether the errors from child objects in the viewmodel will be displayed too.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool IncludeErrorsFromChildren
        {
            get { return (bool) GetValue(IncludeErrorsFromChildrenProperty)!; }
            set { SetValue(IncludeErrorsFromChildrenProperty, value); }
        }
        public static readonly DotvvmProperty IncludeErrorsFromChildrenProperty
            = DotvvmProperty.Register<bool, ValidationSummary>(c => c.IncludeErrorsFromChildren, false);

        /// <summary>
        /// Gets or sets whether this control is hidden if there are no validation messages
        /// </summary>
        public bool HideWhenValid
        {
            get { return (bool)GetValue(HideWhenValidProperty)!; }
            set { SetValue(HideWhenValidProperty, value); }
        }
        public static readonly DotvvmProperty HideWhenValidProperty
            = DotvvmProperty.Register<bool, ValidationSummary>(c => c.HideWhenValid, false);

        /// <summary>
        /// Gets or sets whether the errors from the <see cref="Validation.TargetProperty"/> object will be displayed too.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool IncludeErrorsFromTarget
        {
            get { return (bool) GetValue(IncludeErrorsFromTargetProperty)!; }
            set { SetValue(IncludeErrorsFromTargetProperty, value); }
        }
        public static readonly DotvvmProperty IncludeErrorsFromTargetProperty
            = DotvvmProperty.Register<bool, ValidationSummary>(c => c.IncludeErrorsFromTarget, true);

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
                group.Add("hideWhenValid", HideWhenValid.ToString().ToLowerInvariant());
            }
            writer.AddKnockoutDataBind("dotvvm-validationSummary", group);

            if (HideWhenValid)
            {
                writer.AddStyleAttribute("display", "none");
            }
        }
    }
}
