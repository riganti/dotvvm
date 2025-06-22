using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Resources;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// An utility class that is used to render HTML code.
    /// </summary>
    public class HtmlWriter : IHtmlWriter, IDisposable
    {
        private readonly IDotvvmRequestContext requestContext;
        private readonly bool debug;
        private readonly bool enableWarnings;
        private Utf8StringWriter writer;

        private readonly List<(string name, string? val, string? separator, bool allowAppending)> attributes = [];
        private DotvvmBindableObject? errorContext;
        private Dictionary<string, KnockoutBindingGroup?> dataBindAttributes = [];
        private Utf8StringWriter dataBindAttribute = new Utf8StringWriter(bufferSize: 512);
        private Utf8StringWriter classAttribute = new Utf8StringWriter(bufferSize: 512);
        private Utf8StringWriter styleAttribute = new Utf8StringWriter(bufferSize: 512);
        private HtmlContentStream? contentStream;
        private readonly Stack<string> openTags = new Stack<string>();
        private State state = State.Default;
        private RuntimeWarningCollector WarningCollector => requestContext.Services.GetRequiredService<RuntimeWarningCollector>();

        public static bool IsSelfClosing(string s)
        {
            switch (s)
            {
                case "area":
                case "base":
                case "br":
                case "col":
                case "command":
                case "embed":
                case "hr":
                case "img":
                case "input":
                case "keygen":
                case "link":
                case "meta":
                case "param":
                case "source":
                case "track":
                case "wbr":
                    return true;
                default: return false;
            }
        }

        private static readonly UTF8Encoding encoding = StringUtils.Utf8;

        public HtmlWriter(Stream outputStream, IDotvvmRequestContext requestContext, bool leaveStreamOpen = false)
            : this(new Utf8StringWriter(outputStream, leaveStreamOpen), requestContext)
        {
        }
        internal HtmlWriter(Utf8StringWriter output, IDotvvmRequestContext requestContext)
        {
            this.writer = output;
            this.requestContext = requestContext;
            this.debug = requestContext.Configuration.Debug;
            this.enableWarnings = this.WarningCollector.Enabled;
            this.contentStream = new HtmlContentStream(this);
        }

        internal void Warn(string message, Exception? ex = null)
        {
            Debug.Assert(this.enableWarnings);
            this.WarningCollector.Warn(new DotvvmRuntimeWarning(message, ex, this.errorContext));
        }

        public static string GetSeparatorForAttribute(string attributeName)
        {
            return attributeName switch {
                "class" => " ",
                "data-bind" => ",",
                _ => ";"
            };
        }

        public static string? JoinAttributeValues(string attributeName, string? valueA, string? valueB, string? separator = null)
        {
            if (string.IsNullOrWhiteSpace(valueA))
                return valueB;
            if (string.IsNullOrWhiteSpace(valueB))
                return valueA;

            separator = separator ?? GetSeparatorForAttribute(attributeName);

            // append the value with the separator
            return valueA + separator + valueB;
        }

        /// <summary>
        /// Adds the specified attribute to the next HTML element that is being rendered. 
        /// </summary>
        /// <param name="name">The name of the HTML attribute.</param>
        /// <param name="value">The value of the HTML attribute.</param>
        /// <param name="append">
        ///     If set to false, the value of the attribute will be overwritten. 
        ///     If set to true, the value will be appended to the current attribute value and the <paramref name="appendSeparator"/> will be added when needed.
        /// </param>
        /// <param name="appendSeparator">The separator that will be used when <paramref name="append"/> is true and when the attribute already has a value.</param>
        public void AddAttribute(string name, string? value, bool append = false, string? appendSeparator = null)
        {
            if (name.Equals("class", StringComparison.OrdinalIgnoreCase))
            {
                if (!append) classAttribute.Clear();
                AddClassAttribute(value);
            }
            else if (name.Equals("style", StringComparison.OrdinalIgnoreCase))
            {
                if (!append) styleAttribute.Clear();
                AddStyleAttribute(value);
            }
            else if (name.Equals("data-bind", StringComparison.OrdinalIgnoreCase))
            {
                if (!append) dataBindAttribute.Clear();
                AddKnockoutDataBind(value);
            }
            else
            {
                attributes.Add((name, value, appendSeparator, append));
            }
        }

        public void AddClassAttribute(string? value)
        {
            if (value is null)
                return;

            if (classAttribute.BufferPosition != 0)
                classAttribute.WriteByte((byte)' ');

            classAttribute.Write(value);
        }

        public void AddClassAttribute(ReadOnlySpan<byte> value)
        {
            if (classAttribute.BufferPosition != 0)
                classAttribute.WriteByte((byte)' ');

            classAttribute.Write(value);
        }

        public void AddStyleAttribute(string? nameValue)
        {
            if (nameValue is null)
                return;

            if (styleAttribute.BufferPosition != 0)
                styleAttribute.WriteByte((byte)';');
            styleAttribute.Write(nameValue);
        }

        public void AddStyleAttribute(ReadOnlySpan<byte> nameValue)
        {
            if (styleAttribute.BufferPosition != 0)
                styleAttribute.WriteByte((byte)';');
            styleAttribute.Write(nameValue);
        }

        /// <summary>
        /// Adds the style attribute.
        /// </summary>
        /// <param name="name">The name of the CSS property.</param>
        /// <param name="value">The value of the CSS property.</param>
        public void AddStyleAttribute(string name, string value)
        {
            if (styleAttribute.BufferPosition != 0)
                styleAttribute.WriteByte((byte)';');
            styleAttribute.Write(name);
            styleAttribute.WriteByte((byte)':');
            styleAttribute.Write(value);
        }

        /// <summary>
        /// Adds the data-bind attribute to the next HTML element that is being rendered.
        /// </summary>
        /// <param name="name">The name of the binding handler.</param>
        /// <param name="expression">The binding expression.</param>
        public void AddKnockoutDataBind(string name, string expression)
        {
            if (!dataBindAttributes.TryAdd(name, null))
            {
                if (dataBindAttributes[name] is null)
                    throw new InvalidOperationException($"The binding handler '{name}' is already present on this element.");
                else
                    throw new InvalidOperationException($"The binding handler '{name}' already contains a KnockoutBindingGroup. The expression could not be added. Please call AddKnockoutDataBind(string, KnockoutBindingGroup) overload!");
            }

            if (dataBindAttribute.BufferPosition != 0)
                dataBindAttribute.WriteByte((byte)',', (byte)' ');
            dataBindAttribute.Write(name);
            dataBindAttribute.WriteByte((byte)':', (byte)' ');
            dataBindAttribute.Write(expression);
        }

        private void AddKnockoutDataBind(string? nameAndExpression)
        {
            if (nameAndExpression is null)
                return;

            if (dataBindAttribute.BufferPosition != 0)
                dataBindAttribute.WriteByte((byte)',', (byte)' ');
            dataBindAttribute.Write(nameAndExpression);
        }

        /// <summary>
        /// Adds the data-bind attribute to the next HTML element that is being rendered.
        /// </summary>
        /// <param name="name">The name of the binding handler.</param>
        /// <param name="bindingGroup">A group of name-value pairs.</param>
        public void AddKnockoutDataBind(string name, KnockoutBindingGroup bindingGroup)
        {
            EnsureTagFullyOpen();
            EnsureState(State.Default, State.StreamingContent);
            if (dataBindAttributes.TryGetValue(name, out var exisingGroup))
            {
                if (exisingGroup is null)
                    throw new InvalidOperationException($"The value of binding handler '{name}' cannot be combined with a KnockoutBindingGroup!");
                else
                {
                    exisingGroup.AddFrom(bindingGroup);
                }
            }
            else
            {
                dataBindAttributes[name] = bindingGroup;
            }
        }

        public void WriteKnockoutDataBindComment(string name, string expression)
        {
            if (name.Contains("-->") || expression.Contains("-->"))
                throw new Exception("Knockout data bind comment can't contain substring '-->'. If you have discovered this exception in your log, you probably have a XSS vulnerability in you website.");

            EnsureTagFullyOpen();
            EnsureState(State.Default, State.StreamingContent);

            writer.WriteByte((byte)'<', (byte)'!', (byte)'-', (byte)'-', (byte)' ', (byte)'k', (byte)'o', (byte)' ');
            writer.Write(name);
            writer.WriteByte((byte)':', (byte)' ');
            writer.Write(expression);
            writer.WriteByte((byte)' ', (byte)'-', (byte)'-', (byte)'>');
        }

        public void WriteKnockoutDataBindEndComment()
        {
            EnsureTagFullyOpen();
            EnsureState(State.Default, State.StreamingContent);

            writer.Write("<!-- /ko -->"u8);
        }

        /// <summary>
        /// Renders the begin tag with attributes that were added in <see cref="AddAttribute"/> method.
        /// </summary>
        public void RenderBeginTag(string name)
        {
            RenderBeginTagCore(name);
            state = State.TagOpen;
            openTags.Push(name);
        }

        public void EnsureTagFullyOpen()
        {
            if ((state & State.TagOpen) != 0)
            {
                writer.WriteByte((byte)'>');
                state &= ~State.TagOpen;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureState(State exactlyEqual, [CallerMemberName] string caller = "?")
        {
            if (state != exactlyEqual)
                ThrowStateNotAllowed(exactlyEqual, ~exactlyEqual, caller);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureState(State must, State mustNot, [CallerMemberName] string caller = "?")
        {
            if ((state & mustNot | ~state & must) != 0)
                ThrowStateNotAllowed(must, mustNot, caller);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowStateNotAllowed(State must, State mustNot, string caller)
        {
            var additionalFlags = state & mustNot;
            var missingFlags = must & ~state;
            var message = additionalFlags != 0 && missingFlags != 0 ? $", the state must be {missingFlags} and must not be {additionalFlags}." :
                          additionalFlags != 0 ? $", the state must not be {additionalFlags}." :
                          missingFlags != 0 ? $", the state must be {missingFlags}." : ", unknown error.";
            if ((additionalFlags & State.StreamingContent) != 0)
                throw new InvalidOperationException($"Cannot perform HtmlWriter.{caller}, as the previous {nameof(HtmlContentStream)} wasn't closed yet.");
            if ((additionalFlags & State.HasAttributes) != 0)
                ThrowIfAttributesArePresent(caller);
            throw new Exception($"Cannot perform HtmlWriter.{caller}, as it is in an unexpected state" + message);
        }

        /// <summary>
        /// Renders the self closing tag with attributes that were added in <see cref="AddAttribute"/> method.
        /// </summary>
        public void RenderSelfClosingTag(string name)
        {
            RenderBeginTagCore(name);
            writer.WriteByte((byte)' ', (byte)'/', (byte)'>');
            state = State.Default;

            if (this.enableWarnings && !IsSelfClosing(name))
                Warn($"Element {name} is not self-closing but is rendered as so. It may be interpreted as a start tag without an end tag by the browsers.");
        }

        private Dictionary<string, string?>? attributeMergeTable;

        /// <summary>
        /// Renders the begin tag without end char.
        /// </summary>
        private void RenderBeginTagCore(string name)
        {
            AssertIsValidHtmlName(name);

            EnsureTagFullyOpen();
            EnsureState(State.Default);
            writer.WriteByte((byte)'<');
            writer.Write(name);

#pragma warning disable CS8605
            foreach (var attr in dataBindAttributes)
#pragma warning restore CS8605
            {
                if (attr.Value is { })
                {
                    if (dataBindAttribute.BufferPosition != 0)
                        dataBindAttribute.WriteByte((byte)',', (byte)' ');
                    dataBindAttribute.Write(attr.Key);
                    dataBindAttribute.WriteByte((byte)':', (byte)' ');
                    attr.Value.WriteToUtf8(dataBindAttribute);
                }
            }
            dataBindAttributes.Clear();

            if (attributes.Count == 0)
            {

            }
            else if (attributes.Count == 1)
            {
                var (aname, aval, _, _) = attributes[0];
                // there can't be any name collisions of arguments
                WriteAttrWithTransformers(name, aname, aval);
            }
            else if (attributes.Count == 2 && attributes[0].name != attributes[1].name)
            {
                // there can't be any name collisions

                var (aname, aval, _, _) = attributes[0];
                WriteAttrWithTransformers(name, aname, aval);
                (aname, aval, _, _) = attributes[1];
                WriteAttrWithTransformers(name, aname, aval);
            }
            else if (attributes.Count == 3 && attributes[0].name != attributes[1].name && attributes[0].name != attributes[2].name && attributes[1].name != attributes[2].name)
            {
                var (aname, aval, _, _) = attributes[0];
                WriteAttrWithTransformers(name, aname, aval);
                (aname, aval, _, _) = attributes[1];
                WriteAttrWithTransformers(name, aname, aval);
                (aname, aval, _, _) = attributes[2];
                WriteAttrWithTransformers(name, aname, aval);
            }
            else
            {
                bool hasCollision = false;
                attributeMergeTable ??= new Dictionary<string, string?>(23, StringComparer.OrdinalIgnoreCase);
                foreach (var (aname, aval, separator, append) in attributes)
                {
                    if (attributeMergeTable.TryGetValue(aname, out var oldval))
                    {
                        hasCollision = true;
                        attributeMergeTable[aname] = append ? JoinAttributeValues(aname, oldval, aval, separator) : aval;
                    }
                    else
                    {
                        attributeMergeTable[aname] = aval;
                    }
                }
                if (hasCollision)
                {
                    foreach (var (aname, _, _, _) in attributes)
                    {
                        if (attributeMergeTable.TryGetValue(aname, out var val))
                        {
                            attributeMergeTable.Remove(aname);
                            WriteAttrWithTransformers(name, aname, val);
                        }
                    }
                }
                else
                {
                    foreach (var (aname, aval, _, _) in attributes)
                    {
                        WriteAttrWithTransformers(name, aname, aval);
                    }
                    attributeMergeTable.Clear();
                }
            }
            Debug.Assert(attributeMergeTable is null or { Count: 0 });
            attributes.Clear();

            if (classAttribute.BufferPosition > 0)
            {
                writer.WriteByte((byte)' ', (byte)'c', (byte)'l', (byte)'a', (byte)'s', (byte)'s', (byte)'=');
                WriteAttributeValue(classAttribute.PendingBytes);
                classAttribute.Clear();
            }

            if (styleAttribute.BufferPosition > 0)
            {
                writer.WriteByte((byte)' ', (byte)'s', (byte)'t', (byte)'y', (byte)'l', (byte)'e', (byte)'=');
                WriteAttributeValue(styleAttribute.PendingBytes);
                styleAttribute.Clear();
            }

            if (dataBindAttribute.BufferPosition > 0)
            {
                writer.Write(" data-bind="u8);
                WriteAttributeValue(dataBindAttribute.PendingBytes);
                dataBindAttribute.Clear();
            }
        }

        private void WriteAttrWithTransformers(string name, string attributeName, string? attributeValue)
        {
            // allow to use the attribute transformer
            var pair = new HtmlTagAttributePair() { TagName = name, AttributeName = attributeName };
            if (requestContext.Configuration.Markup.HtmlAttributeTransforms.TryGetValue(pair, out var transformConfiguration))
            {
                // use the transformer
                var transformer = transformConfiguration.GetInstance();
                transformer.RenderHtmlAttribute(this, requestContext, attributeName, attributeValue);
            }
            else
            {
                WriteHtmlAttribute(attributeName, attributeValue);
            }
        }

        /// Throws an exception if the specified string can't be a valid html name.
        /// The point is not to validate according to specification, but to make XSS attacks
        /// impossible - it disables html control characters, but won't throw on digit at the start of the name.
        private void AssertIsValidHtmlName(string name)
        {
            if (name.Length == 0) throw new ArgumentException("HTML name length can't be zero.");
            foreach (var ch in name)
            {
                if (ch == '=' || ch == '"' || ch == '\'' || ch == '<' || ch == '>' || ch == '/' || ch == '&')
                    throw new ArgumentException("HTML control characters are not enabled in names.");
                if (char.IsWhiteSpace(ch))
                    throw new ArgumentException("Whitespace is not allowed in HTML name.");
            }
        }

        public void WriteHtmlAttribute(string attributeName, string? attributeValue)
        {
            // See for context: https://html.spec.whatwg.org/#attributes-2
            writer.WriteByte((byte)' ');
            writer.Write(attributeName);

            // Empty attribute syntax:
            // Just the attribute name. The value is implicitly the empty string.
            if (string.IsNullOrEmpty(attributeValue))
                return;


            writer.WriteByte((byte)'=');

            WriteAttributeValue(attributeValue);
        }

        private void WriteAttributeValue(string value)
        {
            Debug.Assert(value.Length > 0);

            if (CanBeUnquoted(value))
            {
                writer.Write(value);
                return;
            }

            var useQuotes = CountQuotesAndApos(value) <= 0;
            if (useQuotes)
            {
                writer.WriteByte((byte)'"');
                WriteEncodedText(value, escapeQuotes: true, escapeApos: false);
                writer.WriteByte((byte)'"');
            }
            else
            {
                writer.WriteByte((byte)'\'');
                WriteEncodedText(value, escapeQuotes: false, escapeApos: true);
                writer.WriteByte((byte)'\'');
            }
        }

        private void WriteAttributeValue(ReadOnlySpan<byte> value)
        {
            Debug.Assert(value.Length > 0);

            if (CanBeUnquoted(value))
            {
                writer.Write(value);
                return;
            }

            var useQuotes = CountQuotesAndApos(value) <= 0;
            if (useQuotes)
            {
                writer.WriteByte((byte)'"');
                WriteEncodedText(value, escapeQuotes: true, escapeApos: false);
                writer.WriteByte((byte)'"');
            }
            else
            {
                writer.WriteByte((byte)'\'');
                WriteEncodedText(value, escapeQuotes: false, escapeApos: true);
                writer.WriteByte((byte)'\'');
            }
        }

        private bool CanBeUnquoted(string value)
        {
            // The attribute name, followed by zero or more ASCII whitespace, followed by a single U+003D EQUALS SIGN character, followed by zero or more ASCII whitespace, followed by the attribute value, which, in addition to the requirements given above for attribute values, must not contain any literal ASCII whitespace, any U+0022 QUOTATION MARK characters ("), U+0027 APOSTROPHE characters ('), U+003D EQUALS SIGN characters (=), U+003C LESS-THAN SIGN characters (<), U+003E GREATER-THAN SIGN characters (>), or U+0060 GRAVE ACCENT characters (`), and must not be the empty string.
            var length = value.Length;
            if (length > 50) return false;
            for (int i = 0; i < length; i++)
            {
                var ch = value[i];
                if (IsInRange(ch, 'A', 'Z') || IsInRange(ch, 'a', 'z'))
                    continue;
                // Range of -./0123456789:
                if (IsInRange(ch, '-', ':') || ch == '_')
                    continue;

                return false;
            }
            return true;
        }
        private bool CanBeUnquoted(ReadOnlySpan<byte> value)
        {
            var length = value.Length;
            if (length > 50) return false;
            for (int i = 0; i < length; i++)
            {
                var ch = value[i];
                if (IsInRange(ch, (byte)'A', (byte)'Z') || IsInRange(ch, (byte)'a', (byte)'z'))
                    continue;
                // Range of -./0123456789:
                if (IsInRange(ch, (byte)'-', (byte)':') || ch == '_')
                    continue;

                return false;
            }
            return true;
        }

        private int CountQuotesAndApos(string value)
        {
            var length = Math.Min(value.Length, 192);
            var span = value.AsSpan(0, length);
            return span.Count('"') - span.Count('\'');
        }

        private int CountQuotesAndApos(ReadOnlySpan<byte> value)
        {
            var length = Math.Min(value.Length, 192);
            var span = value.Slice(0, length);
            return span.Count((byte)'"') - span.Count((byte)'\'');
        }

        private void WriteEncodedText(ReadOnlySpan<byte> input, bool escapeQuotes, bool escapeApos)
        {
            int index = 0;
            while (true)
            {
                var startIndex = index;
                index = IndexOfHtmlEncodingChars(input, startIndex, escapeQuotes, escapeApos);
                if (index < 0)
                {
                    if (startIndex == 0)
                    {
                        writer.Write(input);
                        return;
                    }
                    writer.Write(input.Slice(startIndex));
                    return;
                }
                else
                {
                    writer.Write(input.Slice(startIndex, index - startIndex));
                    switch (input[index])
                    {
                        case (byte)'"':
                            writer.WriteByte((byte)'&', (byte)'q', (byte)'u', (byte)'o', (byte)'t', (byte)';');
                            break;
                        case (byte)'\'':
                            writer.WriteByte((byte)'&', (byte)'#', (byte)'3', (byte)'9', (byte)';');
                            break;
                        case (byte)'<':
                            writer.WriteByte((byte)'&', (byte)'l', (byte)'t', (byte)';');
                            break;
                        case (byte)'>':
                            writer.WriteByte((byte)'&', (byte)'g', (byte)'t', (byte)';');
                            break;
                        case (byte)'&':
                            writer.WriteByte((byte)'&', (byte)'a', (byte)'m', (byte)'p', (byte)';');
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    index++;
                    if (index == input.Length)
                        return;
                }
            }
        }

        private void WriteEncodedText(string input, bool escapeQuotes, bool escapeApos)
        {
            int index = 0;
            while (true)
            {
                var startIndex = index;
                index = IndexOfHtmlEncodingChars(input, startIndex, escapeQuotes, escapeApos);
                if (index < 0)
                {
                    if (startIndex == 0)
                    {
                        writer.Write(input);
                        return;
                    }
                    writer.Write(input.AsSpan().Slice(startIndex));
                    return;
                }
                else
                {
                    writer.Write(input.AsSpan().Slice(startIndex, index - startIndex));
                    switch (input[index])
                    {
                        case '"':
                            writer.WriteByte((byte)'&', (byte)'q', (byte)'u', (byte)'o', (byte)'t', (byte)';');
                            break;
                        case '\'':
                            writer.WriteByte((byte)'&', (byte)'#', (byte)'3', (byte)'9', (byte)';');
                            break;
                        case '<':
                            writer.WriteByte((byte)'&', (byte)'l', (byte)'t', (byte)';');
                            break;
                        case '>':
                            writer.WriteByte((byte)'&', (byte)'g', (byte)'t', (byte)';');
                            break;
                        case '&':
                            writer.WriteByte((byte)'&', (byte)'a', (byte)'m', (byte)'p', (byte)';');
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    index++;
                    if (index == input.Length)
                        return;
                }
            }
        }
        private static char[] MinimalEscapeChars = ['<', '>', '&'];
        private static byte[] MinimalEscapeBytes = [(byte)'<', (byte)'>', (byte)'&'];
        private static char[] DoubleQEscapeChars = ['<', '>', '&', '"'];
        private static byte[] DoubleQEscapeBytes = [(byte)'<', (byte)'>', (byte)'&', (byte)'"'];
        private static char[] SingleQEscapeChars = ['<', '>', '&', '\''];
        private static byte[] SingleQEscapeBytes = [(byte)'<', (byte)'>', (byte)'&', (byte)'\''];
        private static char[] BothQEscapeChars = ['<', '>', '&', '"', '\''];
        private static byte[] BothQEscapeBytes = [(byte)'<', (byte)'>', (byte)'&', (byte)'"', (byte)'\''];

        private static int IndexOfHtmlEncodingChars(string input, int startIndex, bool escapeQuotes, bool escapeApos)
        {
            char[] breakChars;
            if (escapeQuotes)
            {
                if (escapeApos)
                    breakChars = BothQEscapeChars;
                else
                    breakChars = DoubleQEscapeChars;
            }
            else if (escapeApos)
            {
                breakChars = SingleQEscapeChars;
            }
            else
            {
                breakChars = MinimalEscapeChars;
            }

            int i = startIndex;
            while (true)
            {
                var foundIndex = MemoryExtensions.IndexOfAny(input.AsSpan(start: i), breakChars);
                if (foundIndex < 0)
                    return -1;

                i += foundIndex;

                if (input[i] == '&' && i + 1 < input.Length)
                {
                    // HTML spec permits ampersands, if they are not ambiguous:
                    // (and unnecessarily quoting them makes JS less readable)

                    // An ambiguous ampersand is a U+0026 AMPERSAND character (&) that is followed by one or more ASCII alphanumerics, followed by a U+003B SEMICOLON character (;), where these characters do not match any of the names given in the named character references section.

                    // so if the next character is not alphanumeric, we can leave it there
                    var nextChar = input[i + 1];
                    if (IsInRange(nextChar, 'a', 'z') |
                        IsInRange(nextChar, 'A', 'Z') |
                        IsInRange(nextChar, '0', '9') |
                        nextChar == '#')
                        return i;
                }
                else
                {
                    // all other characters are escaped unconditionaly
                    return i;
                }

                i++;
            }
        }
        private static int IndexOfHtmlEncodingChars(ReadOnlySpan<byte> input, int startIndex, bool escapeQuotes, bool escapeApos)
        {
            byte[] breakChars;
            if (escapeQuotes)
            {
                if (escapeApos)
                    breakChars = BothQEscapeBytes;
                else
                    breakChars = DoubleQEscapeBytes;
            }
            else if (escapeApos)
            {
                breakChars = SingleQEscapeBytes;
            }
            else
            {
                breakChars = MinimalEscapeBytes;
            }

            int i = startIndex;
            while (true)
            {
                var foundIndex = MemoryExtensions.IndexOfAny(input.Slice(start: i), breakChars);
                if (foundIndex < 0)
                    return -1;

                i += foundIndex;

                if (input[i] == '&' && i + 1 < input.Length)
                {
                    // HTML spec permits ampersands, if they are not ambiguous:
                    // (and unnecessarily quoting them makes JS less readable)

                    // An ambiguous ampersand is a U+0026 AMPERSAND character (&) that is followed by one or more ASCII alphanumerics, followed by a U+003B SEMICOLON character (;), where these characters do not match any of the names given in the named character references section.

                    // so if the next character is not alphanumeric, we can leave it there
                    var nextChar = input[i + 1];
                    if (IsInRange(nextChar, (byte)'a', (byte)'z') |
                        IsInRange(nextChar, (byte)'A', (byte)'Z') |
                        IsInRange(nextChar, (byte)'0', (byte)'9') |
                        nextChar == '#')
                        return i;
                }
                else
                {
                    // all other characters are escaped unconditionaly
                    return i;
                }

                i++;
            }
        }

        private void ThrowIfAttributesArePresent([CallerMemberName] string operation = "Write")
        {
            if ((state & State.HasAttributes) != 0)
            {
                var attrs =
                    attributes.Select(a => a.name)
                    .Concat(dataBindAttributes.Keys.OfType<string>().Select(a => "data-bind:" + a))
                    .Concat(dataBindAttribute.PendingBytes.IsEmpty ? [] : ["data-bind"])
                    .Concat(classAttribute.PendingBytes.IsEmpty ? [] : ["class"])
                    .Concat(styleAttribute.PendingBytes.IsEmpty ? [] : ["style"]);
                throw new InvalidOperationException($"Cannot call HtmlWriter.{operation}, since attributes were added into the writer. Attributes: {string.Join(", ", attrs)}");
            }
        }

        // from Char.cs
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsInRange(char c, char min, char max) => (uint)(c - min) <= (uint)(max - min);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsInRange(byte c, byte min, byte max) => (uint)(c - min) <= (uint)(max - min);

        /// <summary>
        /// Renders the end tag.
        /// </summary>
        public void RenderEndTag()
        {
            EnsureState(state & State.TagOpen);
            if (openTags.Count == 0)
            {
                throw new InvalidOperationException("The HtmlWriter cannot close the tag because no tag is open!");
            }

            var tag = openTags.Pop();
            if (state != State.TagOpen || !IsSelfClosing(tag))
            {
                EnsureTagFullyOpen();
                writer.WriteByte((byte)'<', (byte)'/');
                writer.Write(tag);
                writer.WriteByte((byte)'>');

                if (this.enableWarnings && IsSelfClosing(tag))
                    Warn($"Element {tag} is self-closing but contains content. The browser may interpret the start tag as self-closing and put the 'content' into its parent.");
            }
            else
            {
                writer.WriteByte((byte)' ', (byte)'/', (byte)'>');
            }
            state = State.Default;
        }

        /// <summary>
        /// Writes the text.
        /// </summary>
        public void WriteText(string? text, bool escapeQuotes = false, bool escapeApos = false)
        {
            if (text == null || text.Length == 0) return;
            EnsureTagFullyOpen();
            EnsureState(State.Default);
            WriteEncodedText(text, escapeApos, escapeQuotes);
        }

        /// <summary>
        /// Writes the text.
        /// </summary>
        public void WriteText(ReadOnlySpan<byte> textUtf8, bool escapeQuotes = false, bool escapeApos = false)
        {
            if (textUtf8.Length == 0) return;
            EnsureTagFullyOpen();
            EnsureState(State.Default);
            WriteEncodedText(textUtf8, escapeApos, escapeQuotes);
        }

        /// <summary>
        /// Writes the unencoded text.
        /// </summary>
        public void WriteUnencodedText(string? text)
        {
            if (text is null)
                return;
            EnsureTagFullyOpen();
            EnsureState(State.Default);
            writer.Write(text);
        }
        public void WriteUnencodedText(ReadOnlySpan<byte> textUtf8)
        {
            EnsureTagFullyOpen();
            EnsureState(State.Default);
            writer.Write(textUtf8);
        }

        public void WriteUnencodedWhitespace(string? text)
        {
            if (text is null)
                return;
            EnsureTagFullyOpen();
            EnsureState(State.Default, State.StreamingContent);
            writer.Write(text);
        }

        public void WriteUnencodedWhitespace(ReadOnlySpan<byte> textUtf8)
        {
            EnsureTagFullyOpen();
            EnsureState(State.Default, State.StreamingContent);
            writer.Write(textUtf8);
        }

        public void WriteAttributeUnbuffered(ReadOnlySpan<byte> name, ReadOnlySpan<byte> value)
        {
            EnsureState(State.TagOpen, State.StreamingContent);

            writer.WriteByte((byte)' ');
            writer.Write(name);
            if (value.Length == 0)
                return;

            writer.WriteByte((byte)'=');
            WriteAttributeValue(value);
        }
        public void WriteAttributeUnbuffered(ReadOnlySpan<byte> name, string value)
        {
            EnsureState(State.TagOpen, State.StreamingContent);

            writer.WriteByte((byte)' ');
            writer.Write(name);
            if (string.IsNullOrEmpty(value))
                return;

            writer.WriteByte((byte)'=');
            WriteAttributeValue(value);
        }
        public Stream WriteAttributeUnbuffered(ReadOnlySpan<byte> name)
        {
            EnsureState(State.TagOpen, State.StreamingContent);

            writer.WriteByte((byte)' ');
            writer.Write(name);
            writer.WriteByte((byte)'=', (byte)'\'');
            contentStream ??= new(this);
            contentStream.Prepare(encode: true, encodeQuotes: false, encodeApos: true, writeAfter: "'");
            state |= State.StreamingContent;
            return contentStream;
        }

        public void SetErrorContext(DotvvmBindableObject obj) => this.errorContext = obj;

        internal void Reset()
        {
            if (this.openTags.Count > 0)
                Warn($"The HtmlWriter was reset while the following tags are still open: {string.Join(", ", openTags)}");
            this.openTags.Clear();
            this.state = State.Default;
            this.attributes.Clear();
            this.dataBindAttributes.Clear();
            this.dataBindAttribute.Clear();
            this.classAttribute.Clear();
            this.styleAttribute.Clear();
            this.writer.FlushBuffer();
            this.writer.Clear();
            this.errorContext = null;
        }

        public void Dispose()
        {
            if (this.openTags.Count > 0)
            {
                Warn($"The HtmlWriter was disposed while the following tags are still open: {string.Join(", ", openTags)}");
            }
            writer.Dispose();
            styleAttribute.Dispose();
            classAttribute.Dispose();
            dataBindAttribute.Dispose();
        }

        private sealed class HtmlContentStream : Stream
        {
            private readonly HtmlWriter writer;
            public bool Active { get; private set; }
            public bool EncodeQuotes { get; private set; }
            public bool EncodeApos { get; private set; }
            public bool Encode { get; private set; }
            public string WriteAfter { get; private set; } = "";

            public HtmlContentStream(HtmlWriter writer)
            {
                ThrowHelpers.ArgumentNull(writer);
                this.writer = writer;
            }

            public void Prepare(bool encode, bool encodeQuotes, bool encodeApos, string writeAfter = "")
            {
                ThrowIfActive();
                Active = true;
                Encode = encode;
                EncodeQuotes = encodeQuotes;
                EncodeApos = encodeApos;
                WriteAfter = writeAfter;
            }

            public void ThrowIfActive()
            {
                if (Active)
                    ThrowHelperActive();
            }

            private void ThrowHelperActive() => throw new InvalidOperationException($"Previously used {nameof(HtmlContentStream)} was not closed yet.");

            protected override void Dispose(bool disposing)
            {
                if (disposing && Active)
                {
                    Active = false;
                    writer.WriteUnencodedText(WriteAfter);
                }

                base.Dispose(disposing);
            }

            public override void Write(ReadOnlySpan<byte> buffer)
            {
                ThrowHelpers.ObjectDisposed(!Active, this);

                if (buffer.Length == 0)
                    return;

                if (!Encode)
                {
                    writer.WriteUnencodedText(buffer);
                }
                else
                {
                    writer.WriteEncodedText(buffer, EncodeQuotes, EncodeApos);
                }
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                Write(buffer.AsSpan(offset, count));
            }


            public override void WriteByte(byte value)
            {
                Write(new ReadOnlySpan<byte>(ref value));
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                Write(buffer.Span);
                return ValueTask.CompletedTask;
            }

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override void Flush() { }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }

        [Flags]
        enum State: byte
        {
            Default = 0,
            TagOpen = 1, // Tag is still open and new attributes can be appended
            HasAttributes = 2, // The following tag has pending attributes added using AddAttribute method.
            StreamingContent = 4, // The HtmlContentStream is open, no new operations can be performed until it is closed
        }
    }
}
