using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    public class GridViewTextColumn : GridViewColumn
    {


        [MarkupOptions(AllowBinding = false)]
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }
        public static readonly RedwoodProperty FormatStringProperty =
            RedwoodProperty.Register<string, GridViewTextColumn>(c => c.FormatString);



        public override void CreateControls(RedwoodRequestContext context, RedwoodControl container)
        {
            var literal = new Literal();
            literal.FormatString = FormatString;
            literal.SetBinding(Literal.TextProperty, CloneValueBinding());

            container.Children.Add(literal);
        }
    }

    
}
