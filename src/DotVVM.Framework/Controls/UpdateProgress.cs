using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Container for content that will be displayed for the time the page is doing a postback.
    /// </summary>
    public class UpdateProgress : HtmlGenericControl
    {
        /// <summary>
        /// Gets or sets the delay (in ms) after which the content inside UpdateProgress control is shown
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public int Delay
        {
            get { return (int)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        public static readonly DotvvmProperty DelayProperty =
            DotvvmProperty.Register<int, UpdateProgress>(t => t.Delay, 0);

        public UpdateProgress() : base("div")
        {
        }

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            if (Delay<0)
            {
                throw new DotvvmControlException(this,"Delay cannot be set to negative number.");
            }
            base.OnInit(context);
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddKnockoutDataBind("dotvvm-UpdateProgress-Visible", "true");

            if (Delay != 0)
            {
                writer.AddAttribute("data-delay", Delay.ToString());
            }

            base.AddAttributesToRender(writer, context);
        }
    }
}
