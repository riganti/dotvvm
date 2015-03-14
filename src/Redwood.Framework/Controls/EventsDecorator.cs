using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Binding;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    public class EventsDecorator : Decorator
    {

        /// <summary>
        /// Gets or sets the command that will be triggered when the button is pressed.
        /// </summary>
        public Action Click
        {
            get { return (Action)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }
        public static readonly RedwoodProperty ClickProperty =
            RedwoodProperty.Register<Action, EventsDecorator>(t => t.Click, null);


        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            var clickBinding = GetCommandBinding(ClickProperty);
            if (clickBinding != null)
            {
                writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(clickBinding, context, this));
            }

            base.AddAttributesToRender(writer, context);
        }
    }
}
