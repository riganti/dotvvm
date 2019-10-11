using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A GridView column which renders a bool value and can edit it in the CheckBox control.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class GridViewCheckBoxColumn : GridViewValueColumn
    {
        protected override DotvvmControl CreateControl(IDotvvmRequestContext context)
        {
            var checkBox = new CheckBox { Enabled = false };
            checkBox.SetBinding(CheckBox.CheckedProperty, GetValueBinding(ValueBindingProperty));
            return checkBox;
        }

        protected override DotvvmControl CreateEditControl(IDotvvmRequestContext context)
        {
            var checkBox = new CheckBox { Enabled = true };
            checkBox.SetBinding(CheckBox.CheckedProperty, GetValueBinding(ValueBindingProperty));
            return checkBox;
        }
    }
}
