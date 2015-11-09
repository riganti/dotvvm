using System.Collections.Generic;
using DotVVM.Framework.Binding;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A base class for implementations of a postback handler mechanism which can e.g. add an alert before the postback is done.
    /// </summary>
    public abstract class PostBackHandler : DotvvmBindableControl
    {

        /// <summary>
        /// Gets or sets the name of the event which the handler applies to. If this property is not set, it applies to all events.
        /// </summary>
        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }
        public static readonly DotvvmProperty EventNameProperty
            = DotvvmProperty.Register<string, PostBackHandler>(c => c.EventName, null);



        /// <summary>
        /// Gets the key of the handler registered in the dotvvm.extensions.postBackHandlers javascript object.
        /// </summary>
        protected internal abstract string ClientHandlerName { get; }

        /// <summary>
        /// Gets an array of javascript expressions which will be passes to the handler as parameters.
        /// </summary>
        protected internal abstract Dictionary<string, string> GetHandlerOptionClientExpressions();



        protected string TranslateValueOrBinding(DotvvmProperty property)
        {
            var binding = GetValueBinding(property);
            if (binding == null)
            {
                return JsonConvert.SerializeObject(GetValue(property));
            }
            else
            {
                return "ko.unwrap(" + binding.GetKnockoutBindingExpression() + ")";
            }
        }
    }
}