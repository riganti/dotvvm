#nullable enable
using System.Collections.Generic;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Adds a general mechanism to suppress PostBack.
    /// </summary>
    public class SuppressPostBackHandler : PostBackHandler
    {
        protected internal override string ClientHandlerName => "suppress";

        protected internal override Dictionary<string, object?> GetHandlerOptions()
        {
            return new Dictionary<string, object?>() {
                ["suppress"] = this.GetValueRaw(SuppressProperty)
            };
        }

        /// <summary>
        /// Gets or sets the condition to suppress a PostBack.
        /// </summary>
        public bool Suppress
        {
            get { return (bool)GetValue(SuppressProperty)!; }
            set { SetValue(SuppressProperty, value); }
        }
        public static readonly DotvvmProperty SuppressProperty
            = DotvvmProperty.Register<bool, SuppressPostBackHandler>(c => c.Suppress, true);
    }
}
