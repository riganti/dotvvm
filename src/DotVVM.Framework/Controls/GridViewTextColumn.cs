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
        public Command Changed
        {
            get { return (Command)GetValue(ChangedProperty); }
            set { SetValue(ChangedProperty, value); }
        }
        public static readonly DotvvmProperty ChangedProperty =
            DotvvmProperty.Register<Command, GridViewTextColumn>(t => t.Changed, null);

        /// <summary>
        /// Gets or sets the type of value being formatted - Number or DateTime.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public FormatValueType ValueType
        {
            get { return (FormatValueType)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }
        public static readonly DotvvmProperty ValueTypeProperty =
            DotvvmProperty.Register<FormatValueType, GridViewTextColumn>(t => t.ValueType);

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
            literal.ValueType = ValueType;
            literal.SetBinding(Literal.TextProperty, ValueBinding);

            container.Children.Add(literal);
        }

        public override void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            var textBox = new TextBox();
            textBox.FormatString = FormatString;
            textBox.ValueType = ValueType;
            textBox.SetBinding(TextBox.TextProperty, ValueBinding);
            textBox.SetBinding(TextBox.ChangedProperty, GetBinding(ChangedProperty));

            container.Children.Add(textBox);
        }
    }
}
