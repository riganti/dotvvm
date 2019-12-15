#nullable enable
using DotVVM.Framework.Binding;
using System;
namespace DotVVM.Framework.Controls
{
    public class ConcurrencyQueueSetting : DotvvmBindableObject
    {
        /// <summary>
        /// Gets or sets the name of the event which the rule applies to. 
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string? EventName
        {
            get { return (string?)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }
        public static readonly DotvvmProperty EventNameProperty
            = DotvvmProperty.Register<string?, ConcurrencyQueueSetting>(c => c.EventName, null);


        /// <summary>
        /// Gets or sets the name of the concurrency queue that will be used for the specified event.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string ConcurrencyQueue
        {
            get { return (string)GetValue(ConcurrencyQueueProperty)!; }
            set { SetValue(ConcurrencyQueueProperty, value ?? throw new ArgumentNullException(nameof(value))); }
        }
        public static readonly DotvvmProperty ConcurrencyQueueProperty
            = DotvvmProperty.Register<string, ConcurrencyQueueSetting>(c => c.ConcurrencyQueue, "default");

    }
}
