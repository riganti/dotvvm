using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Exceptions;

namespace DotVVM.Framework.Controls
{
    [ControlMarkupOptions(AllowContent = false)]
    public class GridViewTextColumn : GridViewColumn
    {


        [MarkupOptions(AllowBinding = false)]
        public string FormatString { get; set; }


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
            literal.SetBinding(Literal.TextProperty, GetValueBinding(ValueBindingProperty));

            container.Children.Add(literal);
        }
    }

    
}
