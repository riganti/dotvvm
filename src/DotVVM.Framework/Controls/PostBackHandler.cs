using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A base class for implementations of a postback handler mechanism which can e.g. add an alert before the postback is done.
    /// </summary>
    public abstract class PostBackHandler : DotvvmBindableObject
    {
        /// <summary>
        /// Gets or sets the name of the event which the handler applies to. If this property is not set, it applies to all events.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }
        public static readonly DotvvmProperty EventNameProperty
            = DotvvmProperty.Register<string, PostBackHandler>(c => c.EventName, null);

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PostBackHandler"/> is enabled.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty
            = DotvvmProperty.Register<bool, PostBackHandler>(c => c.Enabled, true);

        /// <summary>
        /// Gets the key of the handler registered in the dotvvm.extensions.postBackHandlers javascript object.
        /// </summary>
        protected internal abstract string ClientHandlerName { get; }

        /// <summary>
        /// Gets an array of objects or bindings that will be passed to the postback handler as parameters.
        /// </summary>
        protected internal abstract Dictionary<string, object> GetHandlerOptions();
    }
}