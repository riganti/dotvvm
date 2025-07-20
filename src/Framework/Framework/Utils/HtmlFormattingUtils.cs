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
    public static string DebugHtmlString(this Type? type, bool fullName, bool titleFullName)
    {
        if (type is null)
            return "<span class=syntax-keyword>null</span>";

        if (type.IsGenericParameter)
            return "<span class=syntax-typeparam>" + type.Name + "</span>";

        if (type.IsGenericTypeDefinition)
        {
            var args = type.GetGenericArguments();
            return AddTitleFullName(type, $"{JustName(type, fullName)}<{new string(',', args.Length - 1)}>", titleFullName);
        }

        if (type.IsGenericType)
        {
            if (Nullable.GetUnderlyingType(type) is Type nullableElement)
                return AddTitleFullName(type, nullableElement.DebugHtmlString(fullName, false) + "?", titleFullName);

            var def = type.GetGenericTypeDefinition();
            var args = type.GetGenericArguments();
            var argsHtml = string.Join(", ", args.Select(a => a.DebugHtmlString(fullName, false)));

            if (def.Namespace == "System" && def.Name == "ValueTuple")
                return AddTitleFullName(type, $"({argsHtml})", titleFullName);
            else
                return AddTitleFullName(type, $"{JustName(def, fullName)}<{argsHtml}>", titleFullName);
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var rank = type.GetArrayRank();
            return AddTitleFullName(type, $"{elementType.DebugHtmlString(fullName, false)}[{new string(',', rank - 1)}]", titleFullName);
        }

        if (type.IsByRef)
        {
            var elementType = type.GetElementType()!;
            return AddTitleFullName(type, $"{elementType.DebugHtmlString(fullName, false)}&", titleFullName);
        }

        if (type.IsPointer)
        {
            var elementType = type.GetElementType()!;
            return AddTitleFullName(type, $"{elementType.DebugHtmlString(fullName, false)}*", titleFullName);
        }

        if (type.DeclaringType is { } declaringType)
        {
            var declaringTypeHtml = declaringType.DebugHtmlString(fullName, false);
            return AddTitleFullName(type, $"{declaringTypeHtml}.{JustName(type, false, false)}", titleFullName);
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

        return JustName(type, fullName, titleFullName);



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

    public static string DebugHtmlString(this ITypeDescriptor typeDescriptor, bool fullName, bool titleFullName)
    {
        if (typeDescriptor is ResolvedTypeDescriptor resolvedType)
            return resolvedType.Type.DebugHtmlString(fullName, titleFullName);
        return WebUtility.HtmlEncode(fullName ? typeDescriptor.CSharpFullName : typeDescriptor.CSharpName);
    }

    public sealed class PreformattedHtmlObject : IDebugHtmlFormattableObject
    {
        public string? PlainText { get; }
        public string HtmlText { get; }
        public string HtmlBlockText { get; }
        public PreformattedHtmlObject(string? plainText, string htmlText, string? htmlBlockText = null)
        {
            ThrowHelpers.ArgumentNull(htmlText);

            PlainText = plainText;
            HtmlText = htmlText;
            HtmlBlockText = htmlBlockText ?? htmlText;
        }

        public static PreformattedHtmlObject Create(object? obj)
        {
            var htmlText = TryFormatAsHtml(obj, null, isBlock: false);
            var htmlBlockText = TryFormatAsHtml(obj, null, isBlock: true);
            return new PreformattedHtmlObject(
                plainText: Convert.ToString(obj, null),
                htmlText: htmlText,
                htmlBlockText: htmlBlockText == htmlText ? null : htmlBlockText);
        }

        public PreformattedHtmlObject Append(PreformattedHtmlObject? other) =>
            other is null ? this :
            new(PlainText + other.PlainText,
                HtmlText + other.HtmlText,
                HtmlBlockText + other.HtmlBlockText);

        public PreformattedHtmlObject Append(string plain, string? html = null)
        {
            html ??= WebUtility.HtmlEncode(plain);
            return new(PlainText + plain, HtmlText + html, HtmlBlockText + html);
        }

        public static PreformattedHtmlObject operator +(PreformattedHtmlObject? a, PreformattedHtmlObject? b)
        {
            if (a is null) return b ?? Empty;
            if (b is null) return a;
            return a.Append(b);
        }
        public static PreformattedHtmlObject operator +(PreformattedHtmlObject? a, string? b)
        {
            if (b is null) return a ?? Empty;
            if (a is null) return new PreformattedHtmlObject(b, b);
            return new PreformattedHtmlObject(a.PlainText + b, a.HtmlText + WebUtility.HtmlEncode(b));
        }

        public string DebugHtmlString(IFormatProvider? formatProvider, bool isBlock)
        {
            if (isBlock && HtmlBlockText is { })
                return HtmlBlockText;

            return HtmlText;
        }

        public override string ToString() => PlainText ?? HtmlText;

        public static PreformattedHtmlObject Empty { get; } = new PreformattedHtmlObject("", "");
    }


    public static string TryFormatAsHtml(object? obj, IFormatProvider? formatProvider, bool isBlock, string plainPrefix = "", string plainSuffix = "")
    {
        if (obj is null)
            return "<span class=syntax-keyword>null</span>";

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
