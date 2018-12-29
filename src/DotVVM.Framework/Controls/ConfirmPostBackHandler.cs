using System.Collections.Generic;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Adds a standard javascript confirm before the postback.
    /// </summary>
    public class ConfirmPostBackHandler : PostBackHandler
    {
        protected internal override string ClientHandlerName => "confirm";

        protected internal override Dictionary<string, object> GetHandlerOptions()
        {
            return new Dictionary<string, object>() {
                ["message"] = this.GetValueRaw(MessageProperty)
            };
        }

        /// <summary>
        /// Gets or sets the message of the confirmation dialog.
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public static readonly DotvvmProperty MessageProperty
            = DotvvmProperty.Register<string, ConfirmPostBackHandler>(c => c.Message, null);
    }
}
