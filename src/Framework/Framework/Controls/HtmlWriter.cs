using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
    public class HtmlWriter : IHtmlWriter
    {
        private readonly TextWriter writer;
        private readonly IDotvvmRequestContext requestContext;
        private readonly bool debug;
        private readonly bool enableWarnings;

        private List<(string name, string? val, string? separator, bool allowAppending)> attributes = new List<(string, string?, string? separator, bool allowAppending)>();
        private DotvvmBindableObject? errorContext;
        private OrderedDictionary dataBindAttributes = new OrderedDictionary();
        private Stack<string> openTags = new Stack<string>();
        private bool tagFullyOpen = true;
        private RuntimeWarningCollector WarningCollector => requestContext.Services.GetRequiredService<RuntimeWarningCollector>();

        public static bool IsSelfClosing(string s)
        {
            switch(s)
            {
                case "area":
                case "base":
                case "br" :
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

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlWriter"/> class.
        /// </summary>
        public HtmlWriter(TextWriter writer, IDotvvmRequestContext requestContext)
        {
            this.writer = writer;
            this.requestContext = requestContext;
            this.debug = requestContext.Configuration.Debug;
            this.enableWarnings = this.WarningCollector.Enabled;
        }

        internal void Warn(string message, Exception? ex = null)
        {
            Debug.Assert(this.enableWarnings);
            this.WarningCollector.Warn(new DotvvmRuntimeWarning(message, ex, this.errorContext));
        }

        public static string GetSeparatorForAttribute(string attributeName)
        {
            switch(attributeName)
            {
                case "class": return " ";
                default: return ";";
            }
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
            // if (append)
            // {
            //     if (attributes.Contains(name))
            //     {
            //         var currentValue = attributes[name] as string;
            //         attributes[name] = JoinAttributeValues(name, currentValue, value, appendSeparator);
            //         return;
            //     }
            // }

            attributes.Add((name, value, appendSeparator, append));
        }

        /// <summary>
        /// Adds the style attribute.
        /// </summary>
        /// <param name="name">The name of the CSS property.</param>
        /// <param name="value">The value of the CSS property.</param>
        public void AddStyleAttribute(string name, string value)
        {
            AddAttribute("style", name + ":" + value, true, ";");
        }

        /// <summary>
        /// Adds the data-bind attribute to the next HTML element that is being rendered.
        /// </summary>
        /// <param name="name">The name of the binding handler.</param>
        /// <param name="expression">The binding expression.</param>
        public void AddKnockoutDataBind(string name, string expression)
        {
            if (dataBindAttributes.Contains(name) && dataBindAttributes[name] is KnockoutBindingGroup)
            {
                throw new InvalidOperationException($"The binding handler '{name}' already contains a KnockoutBindingGroup. The expression could not be added. Please call AddKnockoutDataBind(string, KnockoutBindingGroup) overload!");
            }

            dataBindAttributes.Add(name, expression);
        }

        /// <summary>
        /// Adds the data-bind attribute to the next HTML element that is being rendered.
        /// </summary>
        /// <param name="name">The name of the binding handler.</param>
        /// <param name="bindingGroup">A group of name-value pairs.</param>
        public void AddKnockoutDataBind(string name, KnockoutBindingGroup bindingGroup)
        {
            if (dataBindAttributes.Contains(name) && !(dataBindAttributes[name] is KnockoutBindingGroup))
            {
                throw new InvalidOperationException($"The value of binding handler '{name}' cannot be combined with a KnockoutBindingGroup!");
            }

            if (dataBindAttributes.Contains(name))
            {
                var currentGroup = (KnockoutBindingGroup)dataBindAttributes[name];
                currentGroup.AddFrom(bindingGroup);
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

            writer.Write("<!-- ko ");
            writer.Write(name);
            writer.Write(": ");
            writer.Write(expression);
            writer.Write(" -->");
        }

        public void WriteKnockoutDataBindEndComment()
        {
            EnsureTagFullyOpen();

            writer.Write("<!-- /ko -->");
        }

        /// <summary>
        /// Renders the begin tag with attributes that were added in <see cref="AddAttribute"/> method.
        /// </summary>
        public void RenderBeginTag(string name)
        {
            RenderBeginTagCore(name);
            if (IsSelfClosing(name))
            {
                tagFullyOpen = false;
            }
            else
            {
                tagFullyOpen = true;
                writer.Write(">");
            }
            openTags.Push(name);
        }

        public void EnsureTagFullyOpen()
        {
            if (!tagFullyOpen)
            {
                writer.Write(">");
                tagFullyOpen = true;
            }
        }

        /// <summary>
        /// Renders the self closing tag with attributes that were added in <see cref="AddAttribute"/> method.
        /// </summary>
        public void RenderSelfClosingTag(string name)
        {
            RenderBeginTagCore(name);
            writer.Write(" />");

            if (this.enableWarnings && !IsSelfClosing(name))
                Warn($"Element {name} is not self-closing but is rendered as so. It may be interpreted as a start tag without an end tag by the browsers.");
        }

        private Dictionary<string, string?> attributeMergeTable = new Dictionary<string, string?>(23);

        /// <summary>
        /// Renders the begin tag without end char.
        /// </summary>
        private void RenderBeginTagCore(string name)
        {
            writer.Write("<");
            AssertIsValidHtmlName(name);
            writer.Write(name);

#pragma warning disable CS8605
            foreach (DictionaryEntry attr in dataBindAttributes)
#pragma warning restore CS8605
            {
                AddAttribute("data-bind", attr.Key + ": " + ConvertHtmlAttributeValue(attr.Value.NotNull()), true, ", ");
            }
            dataBindAttributes.Clear();

            if (attributes.Count == 0)
                return;

            if (attributes.Count == 1)
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
            else
            {
                bool changed = false;
                foreach (var (aname, aval, separator, append) in attributes)
                {
                    if (attributeMergeTable.TryGetValue(aname, out var oldval))
                    {
                        changed = true;
                        attributeMergeTable[aname] = append ? JoinAttributeValues(aname, oldval, aval, separator) : aval;
                    }
                    else
                    {
                        attributeMergeTable[aname] = aval;
                    }
                }
                if (changed)
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
            Debug.Assert(attributeMergeTable.Count == 0);
            attributes.Clear();
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

            if (this.enableWarnings && char.IsUpper(attributeName[0]))
            {
                Warn($"{attributeName} is used as an HTML attribute on element {name}, but it starts with an uppercase letter. Did you intent to use a DotVVM property instead? To silence this warning, just use all lowercase letters for standard HTML attributes.");
            }
        }

        private string ConvertHtmlAttributeValue(object value)
        {
            if (value is KnockoutBindingGroup koGroup)
            {
                return koGroup.ToString();
            }

            return (string) value;
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
            writer.Write(" ");
            writer.Write(attributeName);

            // Empty attribute syntax:
            // Just the attribute name. The value is implicitly the empty string.
            if (string.IsNullOrEmpty(attributeValue))
                return;


            writer.Write('=');

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
                writer.Write('"');
                WriteEncodedText(value, escapeQuotes: true, escapeApos: false);
                writer.Write('"');
            }
            else
            {
                writer.Write('\'');
                WriteEncodedText(value, escapeQuotes: false, escapeApos: true);
                writer.Write('\'');
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

        private int CountQuotesAndApos(string value)
        {
            // it's not that important we get it right, so limit the search to 190 chars
            var length = Math.Min(value.Length, 190);
            var result = 0;
            for (int i = 0; i < length; i++)
            {
                switch(value[i])
                {
                    case '\'':
                        result--;
                        break;
                    case '"':
                        result++;
                        break;
                }
            }
            return result;
        }


        private void WriteEncodedText(string input, bool escapeQuotes, bool escapeApos)
        {
            int index = 0;
            while (true) {
                var startIndex = index;
                index = IndexOfHtmlEncodingChars(input, startIndex, escapeQuotes, escapeApos);
                if (index < 0)
                {
                    if (startIndex == 0)
                    {
                        writer.Write(input);
                        return;
                    }
                    
#if NoSpan
                    writer.Write(input.Substring(startIndex));
#else
                    writer.Write(input.AsSpan().Slice(startIndex));
#endif
                    return;
                }
                else
                {
#if NoSpan
                    writer.Write(input.Substring(startIndex, index - startIndex));
#else
                    writer.Write(input.AsSpan().Slice(startIndex, index - startIndex));
#endif
                    switch (input[index])
                    {
                        case '<':
                            writer.Write("&lt;");
                            break;
                        case '>':
                            writer.Write("&gt;");
                            break;
                        case '"':
                            writer.Write("&quot;");
                            break;
                        case '\'':
                            writer.Write("&#39;");
                            break;
                        case '&':
                            writer.Write("&amp;");
                            break;
                        default:
                            throw new Exception("Should not happen.");
                    }
                    index++;
                }
            }
        }

        private static int IndexOfHtmlEncodingChars(string input, int startIndex, bool escapeQuotes, bool escapeApos)
        {
            for (int i = startIndex; i < input.Length; i++)
            {
                char ch = input[i];
                if (ch <= '>')
                {
                    switch (ch)
                    {
                        case '<':
                        case '>':
                            return i;
                        case '"':
                            if (escapeQuotes)
                                return i;
                            break;
                        case '\'':
                            if (escapeApos)
                                return i;
                            break;
                        case '&':
                            // HTML spec permits ampersands, if they are not ambiguous:

                            // An ambiguous ampersand is a U+0026 AMPERSAND character (&) that is followed by one or more ASCII alphanumerics, followed by a U+003B SEMICOLON character (;), where these characters do not match any of the names given in the named character references section.

                            // so if the next character is not alphanumeric, we can leave it there
                            if (i == input.Length)
                                return i;
                            var nextChar = input[i + 1];
                            if (IsInRange(nextChar, 'a', 'z') ||
                                IsInRange(nextChar, 'A', 'Z') ||
                                IsInRange(nextChar, '0', '9') ||
                                nextChar == '#')
                                return i;
                            break;
                    }
                }
                else if (char.IsSurrogate(ch))
                {
                    // surrogates are fine, but they must not code for ASCII characters

                    var value = Char.ConvertToUtf32(ch, input[i + 1]);
                    if (value < 256)
                        throw new InvalidOperationException("Encountered UTF16 surrogate coding for ASCII char, this is not allowed.");

                    i++;
                }
            }
 
            return -1;
        }

        private void ThrowIfAttributesArePresent([CallerMemberName] string operation = "Write")
        {
            if (attributes.Count != 0 || dataBindAttributes.Count != 0)
            {
                var attrs =
                    attributes.Select(a => a.name)
                    .Concat(dataBindAttributes.Keys.OfType<string>().Select(a => "data-bind:" + a));
                throw new InvalidOperationException($"Cannot call HtmlWriter.{operation}, since attributes were added into the writer. Attributes: {string.Join(", ", attrs)}");
            }
        }

        // from Char.cs
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsInRange(char c, char min, char max) => (uint)(c - min) <= (uint)(max - min);

        /// <summary>
        /// Renders the end tag.
        /// </summary>
        public void RenderEndTag()
        {
            ThrowIfAttributesArePresent();
            if (openTags.Count == 0)
            {
                throw new InvalidOperationException("The HtmlWriter cannot close the tag because no tag is open!");
            }

            var tag = openTags.Pop();
            if (tagFullyOpen)
            {
                writer.Write("</");
                writer.Write(tag);
                writer.Write(">");

                if (this.enableWarnings && IsSelfClosing(tag))
                    Warn($"Element {tag} is self-closing but contains content. The browser may interpret the start tag as self-closing and put the 'content' into its parent.");
            }
            else
            {
                writer.Write(" />");
                tagFullyOpen = true;
            }
        }

        /// <summary>
        /// Writes the text.
        /// </summary>
        public void WriteText(string? text)
        {
            if (text == null || text.Length == 0) return;
            ThrowIfAttributesArePresent();
            EnsureTagFullyOpen();
            WriteEncodedText(text, escapeApos: false, escapeQuotes: false);
        }

        /// <summary>
        /// Writes the unencoded text.
        /// </summary>
        public void WriteUnencodedText(string? text)
        {
            ThrowIfAttributesArePresent();
            EnsureTagFullyOpen();
            writer.Write(text ?? "");
        }

        public void WriteUnencodedWhitespace(string? text)
        {
            EnsureTagFullyOpen();
            writer.Write(text ?? "");
        }

        public void SetErrorContext(DotvvmBindableObject obj) => this.errorContext = obj;
    }
}
