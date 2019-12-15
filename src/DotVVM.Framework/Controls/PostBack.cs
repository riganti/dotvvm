using System;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class PostBack
    {
        [AttachedProperty(typeof(bool))]
        public static readonly DotvvmProperty UpdateProperty =
            DotvvmProperty.Register<bool, PostBack>(() => UpdateProperty, false);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        [AttachedProperty(typeof(PostBackHandlerCollection))]
        public static readonly DotvvmProperty HandlersProperty =
            DotvvmProperty.Register<PostBackHandlerCollection, PostBack>(() => HandlersProperty, null);

        [MarkupOptions(AllowBinding = false)]
        [AttachedProperty(typeof(PostbackConcurrencyMode))]
        public static readonly DotvvmProperty ConcurrencyProperty =
            DotvvmProperty.Register<PostbackConcurrencyMode, PostBack>(() => ConcurrencyProperty, PostbackConcurrencyMode.Default, isValueInherited: true);

        [MarkupOptions(AllowBinding = false)]
        [AttachedProperty(typeof(string))]
        public static readonly DotvvmProperty ConcurrencyQueueProperty =
            DotvvmProperty.Register<string, PostBack>(() => ConcurrencyQueueProperty, "default", isValueInherited: true);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        [AttachedProperty(typeof(ConcurrencyQueueSettingsCollection))]
        public static readonly DotvvmProperty ConcurrencyQueueSettingsProperty =
            DotvvmProperty.Register<ConcurrencyQueueSettingsCollection, PostBack>(() => ConcurrencyQueueSettingsProperty, null);

    }

    public enum PostbackConcurrencyMode
    {
        Default,
        Deny,
        Queue
    }
}
