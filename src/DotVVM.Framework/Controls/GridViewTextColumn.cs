using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A GridView column which renders a text value (with formatting support) and can edit it in the TextBox control.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class GridViewTextColumn : GridViewColumn
    {
        /// <summary>
        /// Gets or sets the format string that will be applied to numeric or date-time values.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }
        public static readonly DotvvmProperty FormatStringProperty
            = DotvvmProperty.Register<string, GridViewTextColumn>(c => c.FormatString, null);

        /// <summary>
        /// Gets or sets the command that will be triggered when the control text is changed.
        /// </summary>
        public ICommandBinding ChangedBinding
        {
            get { return (ICommandBinding)GetValue(ChangedBindingProperty); }
            set { SetValue(ChangedBindingProperty, value); }
        }
        public static readonly DotvvmProperty ChangedBindingProperty =
            DotvvmProperty.Register<ICommandBinding, GridViewTextColumn>(t => t.ChangedBinding, null);

        /// <summary>
        /// Gets or sets a binding which retrieves the value to display from the current data item.
        /// </summary>
        [MarkupOptions(Required = true)]
        public IValueBinding ValueBinding
        {
            get { return GetValueBinding(ValueBindingProperty); }
            set { SetValue(ValueBindingProperty, value); }
        }
        public static readonly DotvvmProperty ValueBindingProperty =
            DotvvmProperty.Register<IValueBinding, GridViewTextColumn>(c => c.ValueBinding);


        protected override string GetSortExpression()
        {
            if (string.IsNullOrEmpty(SortExpression))
            {
                return ValueBinding?.GetProperty<OriginalStringBindingProperty>()?.Code ??
                    throw new DotvvmControlException(this, $"The 'ValueBinding' property must be set on the '{GetType()}' control!");
            }
            else
            {
                return SortExpression;
            }
        }

        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            var literal = new Literal();
            literal.FormatString = FormatString;
            literal.SetBinding(Literal.TextProperty, ValueBinding);

            container.Children.Add(literal);
        }

        public override void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            var textBox = new TextBox();
            textBox.FormatString = FormatString;
            textBox.SetBinding(TextBox.TextProperty, ValueBinding);
            textBox.SetBinding(TextBox.ChangedProperty, ChangedBinding);

            container.Children.Add(textBox);
        }
    }
}
