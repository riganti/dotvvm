#nullable enable
using System.Linq;
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


        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            var checkBox = new CheckBox { Enabled = false };

            // TODO: rewrite this
            if (Properties.TryGetValue(Properties.First(p => p.Key.ToString() == "UITests.Name").Key, out var property))
            {
                checkBox.Properties.Add(Properties.First(p => p.Key.ToString() == "UITests.Name").Key, property);
            }

            checkBox.SetBinding(CheckBox.CheckedProperty, GetValueBinding(ValueBindingProperty));
            container.Children.Add(checkBox);
        }

        public override void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            var checkBox = new CheckBox { Enabled = true };
            checkBox.SetBinding(CheckBox.CheckedProperty, GetValueBinding(ValueBindingProperty));
            container.Children.Add(checkBox);
        }
    }
}
