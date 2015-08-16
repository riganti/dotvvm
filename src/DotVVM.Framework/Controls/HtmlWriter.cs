using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Resources;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// An utility class that is used to render HTML code.
    /// </summary>
    public class HtmlWriter : IHtmlWriter
    {
        private readonly TextWriter writer;
        private readonly IDotvvmRequestContext requestContext;

        private OrderedDictionary attributes = new OrderedDictionary();
        private Stack<string> openTags = new Stack<string>();
        private bool tagFullyOpen = true;


        private static readonly Dictionary<string, string> separators = new Dictionary<string, string>()
        {
            { "class", " " },
            { "style", ";" }
        };

        public static readonly ISet<string> SelfClosingTags = new HashSet<string>
        {
            "area", "base", "br" , "col", "command", "embed", "hr", "img", "input", "keygen", "link", "meta", "param", "source", "track", "wbr"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlWriter"/> class.
        /// </summary>
        public HtmlWriter(TextWriter writer, IDotvvmRequestContext requestContext)
        {
            this.writer = writer;
            this.requestContext = requestContext;
        }

        public static string JoinAttributeValues(string attributeName, string valueA, string valueB, string separator = null)
        {
            if (string.IsNullOrWhiteSpace(valueA))
                return valueB;
            if (string.IsNullOrWhiteSpace(valueB))
                return valueA;

            if (separator == null && !separators.TryGetValue(attributeName, out separator))
            {
                separator = ";";
            }

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
            if (append)
            {
                if (attributes.Contains(name))
                {
                    var currentValue = attributes[name] as string;
                    attributes[name] = JoinAttributeValues(name, currentValue, value, appendSeparator);
                    return;
                }
            }

            // set the value
            attributes[name] = value;
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
        /// Renders the begin tag with attributes that were added in <see cref="AddAttribute"/> method.
        /// </summary>
        public void RenderBeginTag(string name)
        {
            RenderBeginTagCore(name);
            if (SelfClosingTags.Contains(name))
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
        }

        /// <summary>
        /// Renders the begin tag without end char.
        /// </summary>
        private void RenderBeginTagCore(string name)
        {
            writer.Write("<");
            writer.Write(name);

            //if(openTags.Peek().AutomaticAttributes != null)
            //{
            //    foreach (DictionaryEntry attr in openTags.Peek().AutomaticAttributes)
            //    {
            //        if(attr.Value is IEnumerable)
            //        {
            //            foreach (var val in (IEnumerable)attr.Value)
            //            {
            //                AddAttribute((string)attr.Key, val.ToString(), true);
            //            }
            //        }
            //        else
            //        {
            //            AddAttribute((string)attr.Key, attr.Value.ToString(), true);
            //        }
            //    }
            //}

            if (attributes.Count > 0)
            {
                foreach (DictionaryEntry attr in attributes)
                {
                    var attributeName = (string)attr.Key;
                    var attributeValue = (string)attr.Value;

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
            }

            attributes.Clear();
        }

        public void WriteHtmlAttribute(string attributeName, string attributeValue)
        {
            WriteUnencodedText(" ");
            WriteUnencodedText(attributeName);
            if (attributeValue != null)
            {
                WriteUnencodedText("=");
                WriteUnencodedText("\"");
                WriteText(attributeValue);
                WriteUnencodedText("\"");
            }
        }

        /// <summary>
        /// Renders the end tag.
        /// </summary>
        public void RenderEndTag()
        {
            if (openTags.Count == 0)
            {
                throw new InvalidOperationException(Parser_Dothtml.HtmlWriter_CannotCloseTagBecauseNoTagIsOpen);
            }

            var tag = openTags.Pop();
            if (tagFullyOpen)
            {
                writer.Write("</");
                writer.Write(tag);
                writer.Write(">");
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
            EnsureTagFullyOpen();
            writer.Write(WebUtility.HtmlEncode(text).Replace("\"", "&quot;"));
        }

        /// <summary>
        /// Writes the unencoded text.
        /// </summary>
        public void WriteUnencodedText(string text)
        {
            EnsureTagFullyOpen();
            writer.Write(text);
        }

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