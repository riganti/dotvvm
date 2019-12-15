using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Resources;
using DotVVM.Framework.Runtime;
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

        private DotvvmBindableObject errorContext;
        private List<(string name, string val, string separator, bool allowAppending)> attributes = new List<(string, string, string separator, bool allowAppending)>();
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

        internal void Warn(string message, Exception ex = null)
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

        public static string JoinAttributeValues(string attributeName, string valueA, string valueB, string separator = null)
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
        public void AddAttribute(string name, string value, bool append = false, string appendSeparator = null)
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
            writer.Write("/>");

            if (this.enableWarnings && !IsSelfClosing(name))
                Warn($"Element {name} is not self-closing but is rendered as so. It may be interpreted as a start tag without an end tag by the browsers.");
        }

        private Dictionary<string, string> attributeMergeTable = new Dictionary<string, string>(23);

        /// <summary>
        /// Renders the begin tag without end char.
        /// </summary>
        private void RenderBeginTagCore(string name)
        {
            writer.Write("<");
            AssertIsValidHtmlName(name);
            writer.Write(name);

            foreach (DictionaryEntry attr in dataBindAttributes)
            {
                AddAttribute("data-bind", attr.Key + ": " + ConvertHtmlAttributeValue(attr.Value), true, ", ");
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

        private void WriteAttrWithTransformers(string name, string attributeName, string attributeValue)
        {
            // allow to use the attribute transformer
            var pair = new HtmlTagAttributePair() { TagName = name, AttributeName = attributeName };
            HtmlAttributeTransformConfiguration transformConfiguration;
            if (requestContext.Configuration.Markup.HtmlAttributeTransforms.TryGetValue(pair, out transformConfiguration))
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

        private string ConvertHtmlAttributeValue(object value)
        {
            if (value is KnockoutBindingGroup)
            {
                return value.ToString();
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

        public void WriteHtmlAttribute(string attributeName, string attributeValue)
        {
            writer.Write(" ");
            writer.Write(attributeName);
            if (attributeValue != null)
            {
                if (this.debug)
                {
                    writer.Write("=");
                    // this is only for debug, as I'm not sure about performance and security implications
                    // TODO: make this production ready (including proper performance comparison and security analysis)
                    var (singleCount, doubleCount) = (attributeValue.Count(c => c == '\''), attributeValue.Count(c => c == '"'));
                    var separator = singleCount > doubleCount ? "\"" : "'";
                    writer.Write(separator);
                    writer.Write(
                        (separator == "'" ? attributeValue.Replace("&", "&amp;").Replace("'", "&#39;") : attributeValue.Replace("&", "&amp;").Replace("\"", "&quot;"))
                        .Replace(">", "&gt;").Replace("<", "&lt;"));
                    writer.Write(separator.ToString());
                } else
                {
                    writer.Write("=\"");
                    WriteText(attributeValue);
                    writer.Write("\"");
                }
            }
        }

        /// <summary>
        /// Renders the end tag.
        /// </summary>
        public void RenderEndTag()
        {
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
        public void WriteText(string text)
        {
            if (text == null && text.Length == 0) return;
            EnsureTagFullyOpen();
            WebUtility.HtmlEncode(text, this.writer);
        }

        /// <summary>
        /// Writes the unencoded text.
        /// </summary>
        public void WriteUnencodedText(string text)
        {
            EnsureTagFullyOpen();
            writer.Write(text ?? "");
        }

        public void SetErrorContext(DotvvmBindableObject obj) => this.errorContext = obj;
    }
    public class HtmlElementInfo
    {
        public string Name { get; internal set; }
        private Dictionary<string, object> properties;

        public void SetProperty(string name, object value)
        {
            if (properties == null) properties = new Dictionary<string, object>();
            properties[name] = value;
        }
        public object GetProperty(string name)
        {
            if (properties == null) return null;
            object result = null;
            properties.TryGetValue(name, out result);
            return result;
        }
    }
}
