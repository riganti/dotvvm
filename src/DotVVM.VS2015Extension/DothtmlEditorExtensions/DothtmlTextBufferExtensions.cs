using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions
{
    public static class DothtmlTextBufferExtensions
    {
        public const string TextBufferProjectionSpansProperty = "TextBufferProjectionSpansProperty";
        public const string TextBufferProjectionBuffersProperty = "TextBufferProjectionBuffersProperty";

        public static void SetProperty<T>(this ITextBuffer buffer, object key, T value) where T : class
        {
            buffer.Properties.GetOrCreateSingletonProperty(key, () => default(T));
            buffer.Properties[key] = value;
        }

        public static T GetProperty<T>(this ITextBuffer buffer, object key) where T : class
        {
            return buffer.Properties.GetProperty(key) as T;
        }
    }
}