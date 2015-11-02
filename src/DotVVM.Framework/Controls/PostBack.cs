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
            DotvvmProperty.Register<bool, PostBack>("Update", false);

        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        [AttachedProperty(typeof(PostBackHandlerCollection))]
        public static readonly DotvvmProperty HandlersProperty =
            DotvvmProperty.Register<PostBackHandlerCollection, PostBack>("Handlers", null);
    }
}
