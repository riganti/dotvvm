using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;

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
        public string FormatString { get; set; }

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
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public object ValueBinding
        {
            get { return GetValue(ValueBindingProperty); }
            set { SetValue(ValueBindingProperty, value); }
        }
        public static readonly DotvvmProperty ValueBindingProperty =
            DotvvmProperty.Register<object, GridViewTextColumn>(c => c.ValueBinding);


        protected override string GetSortExpression()
        {
            if (string.IsNullOrEmpty(SortExpression))
            {
                var valueBinding = GetValueBinding(ValueBindingProperty) as ValueBindingExpression;
                if (valueBinding != null)
                {
                    return valueBinding.OriginalString;
                }
                else
                {
                    throw new DotvvmControlException(this, $"The 'ValueBinding' property must be set on the '{GetType()}' control!");
                }
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
            literal.SetBinding(Literal.TextProperty, GetValueBinding(ValueBindingProperty));

            container.Children.Add(literal);
        }

        public override void CreateEditControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            var textBox = new TextBox();
            textBox.FormatString = FormatString;
            textBox.ValueType = ValueType;
            textBox.SetBinding(TextBox.TextProperty, GetValueBinding(ValueBindingProperty));

            container.Children.Add(textBox);
        }
    }
    
}
