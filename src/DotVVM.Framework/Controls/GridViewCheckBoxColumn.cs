#nullable enable
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A GridView column which renders a bool value and can edit it in the CheckBox control.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class GridViewCheckBoxColumn : GridViewColumn
    {
        /// <summary>
        /// Gets or sets a binding which retrieves the value to display from the current data item.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public bool ValueBinding
        {
            get { return (bool)GetValue(ValueBindingProperty)!; }
            set { SetValue(ValueBindingProperty, value); }
        }
        public static readonly DotvvmProperty ValueBindingProperty =
            DotvvmProperty.Register<bool, GridViewCheckBoxColumn>(c => c.ValueBinding);

        public ValidatorPlacement ValidatorPlacement
        {
            get { return (ValidatorPlacement)GetValue(ValidatorPlacementProperty)!; }
            set { SetValue(ValidatorPlacementProperty, value); }
        }
        public static readonly DotvvmProperty ValidatorPlacementProperty
            = DotvvmProperty.Register<ValidatorPlacement, GridViewCheckBoxColumn>(c => c.ValidatorPlacement, default);

        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            CreateControlsCore(container, enabled: false);
        }

        public override void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            CreateControlsCore(container, enabled: true);
        }

        private void CreateControlsCore(DotvvmControl container, bool enabled)
        {
            var checkBox = new CheckBox { Enabled = enabled };
            var valueBinding = GetValueBinding(ValueBindingProperty);
            checkBox.SetBinding(CheckBox.CheckedProperty, valueBinding);
            Validator.Place(checkBox, container.Children, valueBinding, ValidatorPlacement);
            container.Children.Add(checkBox);
        }
    }
}
