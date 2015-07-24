using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    public class EventsDecorator : Decorator
    {

        /// <summary>
        /// Gets or sets the command that will be triggered when the button is pressed.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public Action Click
        {
            get { return (Action)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }
        public static readonly DotvvmProperty ClickProperty =
            DotvvmProperty.Register<Action, EventsDecorator>(t => t.Click, null);

        /// <summary>
        /// Gets or sets the command that will be triggered when the value changes.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public Action ValueChanged
        {
            get { return (Action)GetValue(ValueChangedProperty); }
            set { SetValue(ValueChangedProperty, value); }
        }
        public static readonly DotvvmProperty ValueChangedProperty =
            DotvvmProperty.Register<Action, EventsDecorator>(t => t.Click, null);


        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            AddEventAttributeToRender(writer, context, "onclick", ClickProperty);

            AddEventAttributeToRender(writer, context, "onchange", ValueChangedProperty);

            base.AddAttributesToRender(writer, context);
        }

        private void AddEventAttributeToRender(IHtmlWriter writer, RenderContext context, string name, DotvvmProperty property)
        {
            var binding = GetCommandBinding(property);
            if (binding != null)
            {
                writer.AddAttribute(name, KnockoutHelper.GenerateClientPostBackScript(binding, context, this));
            }
        }
    }
}
