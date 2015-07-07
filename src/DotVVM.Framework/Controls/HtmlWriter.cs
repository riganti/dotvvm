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
        private readonly DotvvmRequestContext requestContext;

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
        public HtmlWriter(TextWriter writer, DotvvmRequestContext requestContext)
        {
            this.writer = writer;
            this.requestContext = requestContext;
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
                    if (!string.IsNullOrWhiteSpace(currentValue))
                    {
                        if (appendSeparator == null && !separators.TryGetValue(name, out appendSeparator))
                        {
                            appendSeparator = ";";
                        }

                        // append the value with the separator
                        if (value != null)
                        {
                            attributes[name] = currentValue + appendSeparator + value;
                        }
                        return;
                    }
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
            if(!tagFullyOpen)
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
            
            var name = openTags.Pop();
            if (tagFullyOpen)
            {
                writer.Write("</");
                writer.Write(name);
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
}