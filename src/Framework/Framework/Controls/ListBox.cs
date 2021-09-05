using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the HTML list box.
    /// </summary>
    public class ListBox : SelectHtmlControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        public ListBox()
        {
            
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.AddAttributesToRender(writer, context);
            writer.AddKnockoutDataBind("size", this, SizeProperty, () => writer.AddAttribute("size", Size.ToString()));
        }

        /// <summary>
        /// Gets or sets number of rows visible in this ListBox.
        /// </summary>
        public int Size
        {
            get { return (int)GetValue(SizeProperty)!; }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DotvvmProperty SizeProperty =
            DotvvmProperty.Register<int, ListBox>(t => t.Size, defaultValue: 10);
    }
}
