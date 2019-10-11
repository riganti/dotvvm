using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A base class for GridView columns that present a value by wrapping a control.
    /// </summary>
    public abstract class GridViewValueColumn : GridViewColumn
    {
        /// <summary>
        /// Gets or sets a binding which retrieves the value to display from the current data item.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public IValueBinding ValueBinding
        {
            get { return (IValueBinding)GetValue(ValueBindingProperty); }
            set { SetValue(ValueBindingProperty, value); }
        }
        public static readonly DotvvmProperty ValueBindingProperty
            = DotvvmProperty.Register<IValueBinding, GridViewValueColumn>(c => c.ValueBinding);

        [MarkupOptions(AllowBinding = false)]
        public bool IsControlValidated
        {
            get { return (bool)GetValue(IsControlValidatedProperty); }
            set { SetValue(IsControlValidatedProperty, value); }
        }
        public static readonly DotvvmProperty IsControlValidatedProperty
            = DotvvmProperty.Register<bool, GridViewValueColumn>(c => c.IsControlValidated, false);

        [MarkupOptions(AllowBinding = false)]
        public bool IsValidatorStandalone
        {
            get { return (bool)GetValue(IsValidatorStandaloneProperty); }
            set { SetValue(IsValidatorStandaloneProperty, value); }
        }
        public static readonly DotvvmProperty IsValidatorStandaloneProperty
            = DotvvmProperty.Register<bool, GridViewValueColumn>(c => c.IsValidatorStandalone, false);

        protected abstract DotvvmControl CreateControl(IDotvvmRequestContext context);

        protected abstract DotvvmControl CreateEditControl(IDotvvmRequestContext context);

        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            var wrappedControl = CreateControl(context);
            CreateValidator(container, wrappedControl);
            container.Children.Add(wrappedControl);
        }

        public override void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            var wrappedControl = CreateEditControl(context);
            CreateValidator(container, wrappedControl);
            container.Children.Add(wrappedControl);
        }

        private void CreateValidator(DotvvmControl container, DotvvmControl wrappedControl)
        {
            if (IsControlValidated)
            {
                if (IsValidatorStandalone)
                {
                    var validator = new Validator();
                    validator.SetBinding(Validator.ValueProperty, ValueBinding);
                    container.Children.Add(validator);
                }
                else
                {
                    wrappedControl.SetBinding(Validator.ValueProperty, ValueBinding);
                }
            }
        }
    }
}
