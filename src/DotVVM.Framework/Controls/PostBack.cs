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

        public static void AddHandler(DotvvmControl control, PostBackHandler handler)
        {
            var collection = (PostBackHandlerCollection)control.GetValue(HandlersProperty) ?? new PostBackHandlerCollection();
            collection.Add(handler);
            control.SetValue(HandlersProperty, collection);
        }
    }
}
