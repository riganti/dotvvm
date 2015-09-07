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
        public string FormatString { get; set; }



        public override void CreateControls(IDotvvmRequestContext context, DotvvmControl container)
        {
            var literal = new Literal();
            literal.FormatString = FormatString;
            literal.SetBinding(Literal.TextProperty, GetValueBinding(ValueBindingProperty));

            container.Children.Add(literal);
        }
    }

    
}
