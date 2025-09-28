using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

internal static class HtmlFormattingUtils
{
    public static string DebugHtmlString(this Type? type, bool fullName, bool? titleFullName = null)
    {
        var addTitle = titleFullName ?? !fullName;
        if (type is null)
            return "<span class=syntax-keyword>null</span>";

        if (type.IsGenericParameter)
            return "<span class=syntax-typeparam>" + type.Name + "</span>";

        if (type.IsGenericTypeDefinition)
        {
            var args = type.GetGenericArguments();
            return AddTitleFullName(type, $"{JustName(type, fullName)}<{new string(',', args.Length - 1)}>", addTitle);
        }

        if (type.IsGenericType)
        {
            if (Nullable.GetUnderlyingType(type) is Type nullableElement)
                return AddTitleFullName(type, nullableElement.DebugHtmlString(fullName, false) + "?", addTitle);

            var def = type.GetGenericTypeDefinition();
            var args = type.GetGenericArguments();
            var argsHtml = string.Join(", ", args.Select(a => a.DebugHtmlString(fullName, false)));

            if (def.Namespace == "System" && def.Name == "ValueTuple")
                return AddTitleFullName(type, $"({argsHtml})", addTitle);
            else
                return AddTitleFullName(type, $"{JustName(def, fullName)}<{argsHtml}>", addTitle);
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var rank = type.GetArrayRank();
            return AddTitleFullName(type, $"{elementType.DebugHtmlString(fullName, false)}[{new string(',', rank - 1)}]", addTitle);
        }

        if (type.IsByRef)
        {
            var elementType = type.GetElementType()!;
            return AddTitleFullName(type, $"{elementType.DebugHtmlString(fullName, false)}&", addTitle);
        }

        if (type.IsPointer)
        {
            var elementType = type.GetElementType()!;
            return AddTitleFullName(type, $"{elementType.DebugHtmlString(fullName, false)}*", addTitle);
        }

        if (type.DeclaringType is { } declaringType)
        {
            var declaringTypeHtml = declaringType.DebugHtmlString(fullName, false);
            return AddTitleFullName(type, $"{declaringTypeHtml}.{JustName(type, false, false)}", addTitle);
        }

        if (type == typeof(object))
                return "<span class=syntax-keyword>object</span>";
        if (type == typeof(void))
            return "<span class=syntax-keyword>void</span>";
        if (type == typeof(string))
            return "<span class=syntax-keyword>string</span>";
        if (type == typeof(char))
            return "<span class=syntax-keyword>char</span>";
        if (type == typeof(decimal))
            return "<span class=syntax-keyword>decimal</span>";
        if (type == typeof(bool))
            return "<span class=syntax-keyword>bool</span>";
        if (type == typeof(sbyte))
            return "<span class=syntax-keyword>sbyte</span>";
        if (type == typeof(byte))
            return "<span class=syntax-keyword>byte</span>";
        if (type == typeof(short))
            return "<span class=syntax-keyword>short</span>";
        if (type == typeof(ushort))
            return "<span class=syntax-keyword>ushort</span>";
        if (type == typeof(int))
            return "<span class=syntax-keyword>int</span>";
        if (type == typeof(uint))
            return "<span class=syntax-keyword>uint</span>";
        if (type == typeof(long))
            return "<span class=syntax-keyword>long</span>";
        if (type == typeof(ulong))
            return "<span class=syntax-keyword>ulong</span>";
        if (type == typeof(IntPtr))
            return "<span class=syntax-keyword>nint</span>";
        if (type == typeof(UIntPtr))
            return "<span class=syntax-keyword>nuint</span>";
        if (type == typeof(float))
            return "<span class=syntax-keyword>float</span>";
        if (type == typeof(double))
            return "<span class=syntax-keyword>double</span>";

        return JustName(type, fullName, addTitle);



        static string TitleFullName(Type t, bool condition = true) =>
                                                condition ? $" title='{WebUtility.HtmlEncode(t.ToCode())}'" : "";
        static string AddTitleFullName(Type t, string html, bool condition = true)
        {
            if (condition)
            {
                var title = WebUtility.HtmlEncode(t.ToCode(stripNamespace: false));
                if (title != html)
                {
                    return $"<span title='{title}'>{html}</span>";
                }
            }

            return html;
        }
        static string JustName(Type t, bool fullName, bool titleFullName = false)
        {
            var name = t.Name;
            if (t.IsGenericType && name.LastIndexOf('`') > 0)
            {
                name = name.Substring(0, name.LastIndexOf('`'));
            }

            var className = t.IsInterface ? "syntax-interface" : "syntax-class";
            if (fullName && !string.IsNullOrEmpty(t.Namespace))
            {
                return $"<span class=syntax-prop>{t.Namespace}</span>.<span class={className}>{WebUtility.HtmlEncode(name)}</span>";
            }
            else
            {
                return $"<span class={className} {TitleFullName(t, titleFullName)}>{WebUtility.HtmlEncode(name)}</span>";
            }
        }
    }

    public static string DebugHtmlString(this ITypeDescriptor typeDescriptor, bool fullName, bool? titleFullName = null)
    {
        if (ResolvedTypeDescriptor.TryToSystemType(typeDescriptor) is {} systemType)
            return systemType.DebugHtmlString(fullName, titleFullName);
        return WebUtility.HtmlEncode(fullName ? typeDescriptor.CSharpFullName : typeDescriptor.CSharpName);
    }

    public static IDebugHtmlFormattableObject AsDebugHtmlFormattable(this Type type, bool? fullName = null, bool? titleFullName = null) =>
        AsHtmlFormattable((type, fullName, titleFullName),
            static (x, isBlock) => x.type.DebugHtmlString(x.fullName ?? isBlock, x.titleFullName),
            static x => x.type.ToCode(stripNamespace: x.fullName == false));

    public static IDebugHtmlFormattableObject AsDebugHtmlFormattable(this ITypeDescriptor type, bool? fullName = null, bool? titleFullName = null) =>
        AsHtmlFormattable((type, fullName, titleFullName),
            static (x, isBlock) => x.type.DebugHtmlString(x.fullName ?? isBlock, x.titleFullName),
            static x => x.fullName == false ? x.type.CSharpName : x.type.CSharpFullName);

    public sealed class CompositeHtmlFormattableObject : IDebugHtmlFormattableObject
    {
        private readonly IReadOnlyList<object?> children;
        private readonly bool? makeBlock;

        public CompositeHtmlFormattableObject(IReadOnlyList<object?> children, bool? makeBlock = null)
        {
            this.children = children;
            this.makeBlock = makeBlock;
        }

        private List<(object? obj, bool isBlock)> FlattenHierarchy(List<(object?, bool)> list, bool isBlock)
        {
            foreach (var c in children)
            {
                if (c is CompositeHtmlFormattableObject composite)
                    composite.FlattenHierarchy(list, makeBlock ?? isBlock);
                else
                    list.Add((c, makeBlock ?? isBlock));
            }
            return list;
        }

        public string DebugHtmlString(IFormatProvider? formatProvider, bool isBlock) =>
            string.Concat(
                FlattenHierarchy([], isBlock)
                .Select(o => TryFormatAsHtml(o.obj, formatProvider, o.isBlock))
            );

        public override string ToString() => string.Concat(FlattenHierarchy([], true).Select(o => o.obj ?? "null"));
    }

    public sealed class PreformattedHtmlObject : IDebugHtmlFormattableObject
    {
        public string PlainText { get; }

        private string? _htmlText;
        public string HtmlText => _htmlText ??= WebUtility.HtmlEncode(PlainText);

        private readonly string? _htmlBlockText;
        public string HtmlBlockText => _htmlBlockText ?? HtmlText;

        public PreformattedHtmlObject(string plainText, string? htmlText = null, string? htmlBlockText = null)
        {
            ThrowHelpers.ArgumentNull(htmlText);

            PlainText = plainText;
            _htmlText = htmlText;
            _htmlBlockText = htmlBlockText ?? htmlText;
        }

        public static IDebugHtmlFormattableObject Create(string? obj) =>
            string.IsNullOrEmpty(obj) ? Empty : new PreformattedHtmlObject(obj);
        public static IDebugHtmlFormattableObject Create(object? obj)
        {
            if (obj is IDebugHtmlFormattableObject result)
                return result;
            if (obj is string str)
                return new PreformattedHtmlObject(str, null);
            return new CompositeHtmlFormattableObject([ obj ]);
        }

        public static IDebugHtmlFormattableObject operator +(PreformattedHtmlObject? a, IDebugHtmlFormattableObject? b) => a.Append(b);
        public static IDebugHtmlFormattableObject operator +(PreformattedHtmlObject? a, string? b)
        {
            if (b is null) return a ?? Empty;
            if (a is null) return new PreformattedHtmlObject(b, b);
            return new CompositeHtmlFormattableObject([ a, b ]);
        }

        public string DebugHtmlString(IFormatProvider? formatProvider, bool isBlock) =>
            isBlock ? HtmlBlockText : HtmlText;

        public override string ToString() => PlainText ?? HtmlText;

        public static PreformattedHtmlObject Empty { get; } = new PreformattedHtmlObject("", "");
    }

    public sealed class CustomHtmlFormattable<T>: IDebugHtmlFormattableObject
    {
        public T Object { get; }
        public Func<T, bool, string> ToHtml { get; }
        public Func<T, string>? ToPlainString { get; }
        public CustomHtmlFormattable(T obj, Func<T, bool, string> toHtml, Func<T, string>? toString = null)
        {
            Object = obj;
            ToHtml = toHtml;
            ToPlainString = toString;
        }

        public string DebugHtmlString(IFormatProvider? formatProvider, bool isBlock) =>
            ToHtml(Object, isBlock);

        public override string ToString() => ToPlainString?.Invoke(Object) ?? Object?.ToString() ?? "null";
    }

    public static IDebugHtmlFormattableObject Append(this IDebugHtmlFormattableObject? a, IDebugHtmlFormattableObject? b)
    {
        if (a is null || a == PreformattedHtmlObject.Empty) return b ?? PreformattedHtmlObject.Empty;
        if (b is null || b == PreformattedHtmlObject.Empty) return a;
        return new CompositeHtmlFormattableObject([ a, b ]);
    }

    public static IDebugHtmlFormattableObject Append(this IDebugHtmlFormattableObject? a, string? b)
    {
        if (a is null || a == PreformattedHtmlObject.Empty) return PreformattedHtmlObject.Create(b);
        if (b is null or "") return a;
        return new CompositeHtmlFormattableObject([ a, b ]);
    }

    public static IDebugHtmlFormattableObject Append(this IDebugHtmlFormattableObject? a, string b, string bHtml)
    {
        if (a is null || a == PreformattedHtmlObject.Empty) return new PreformattedHtmlObject(b, bHtml);
        if (b is "" && bHtml is "") return a;
        return new CompositeHtmlFormattableObject([ a, new PreformattedHtmlObject(b, bHtml) ]);
    }

    public static IDebugHtmlFormattableObject AsHtmlFormattable<T>(T obj, Func<T, bool, string> toHtml, Func<T, string>? toString = null) =>
        new CustomHtmlFormattable<T>(obj, toHtml, toString);

    public static string TryFormatAsHtml(object? obj, IFormatProvider? formatProvider, bool isBlock, string plainPrefix = "", string plainSuffix = "")
    {
        if (obj is null)
            return "<span class=syntax-keyword>null</span>";
        if (obj is string str)
            return plainPrefix + WebUtility.HtmlEncode(str) + plainSuffix;

        try
        {
            if (obj is IDebugHtmlFormattableObject htmlFormattable)
                return htmlFormattable.DebugHtmlString(formatProvider, isBlock);

            if (obj is Type type)
                return type.DebugHtmlString(fullName: isBlock, titleFullName: true);

            if (obj is ITypeDescriptor typeDescriptor)
                return typeDescriptor.DebugHtmlString(fullName: isBlock, titleFullName: true);
        }
        catch (Exception ex)
        {
            Debug.Assert(false, "Error while formatting object as HTML: " + ex.ToString());
        }

        var enumerable = obj as IEnumerable<object> ?? (obj as IEnumerable)?.Cast<object>();
        if (enumerable is {} && obj is not string)
        {
            var items = enumerable.Select(o => TryFormatAsHtml(o, formatProvider, isBlock, plainPrefix, plainSuffix)).ToArray();
            if (items.Length == 0)
                return "[]";
            if (isBlock)
                return "<ul>" + string.Concat(items.Select(i => "<li>" + i + "</li>")) + "</ul>";
            else
                return "[ " + string.Join(", ", items) + " ]";
        }


        try
        {
            return plainPrefix + WebUtility.HtmlEncode((
                obj is Exception ex ? ex.Message
                                    : Convert.ToString(obj, formatProvider)) ?? "") + plainSuffix;
        }
        catch (Exception ex)
        {
            Debug.Assert(false, "Error while formatting object as HTML: " + ex.ToString());
            return "" + WebUtility.HtmlEncode(ex.Message) + "</span>";
        }
    }
}
