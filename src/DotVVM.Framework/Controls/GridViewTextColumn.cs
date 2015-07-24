using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    public class GridViewTextColumn : GridViewColumn
    {


        [MarkupOptions(AllowBinding = false)]
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }
        public static readonly DotvvmProperty FormatStringProperty =
            DotvvmProperty.Register<string, GridViewTextColumn>(c => c.FormatString);



        public override void CreateControls(DotvvmRequestContext context, DotvvmControl container)
        {
            var literal = new Literal();
            literal.FormatString = FormatString;
            literal.SetBinding(Literal.TextProperty, CloneValueBinding());

            container.Children.Add(literal);
        }
    }

    
}
