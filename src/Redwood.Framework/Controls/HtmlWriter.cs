using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using Redwood.Framework.Resources;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// An utility class that is used to render HTML code.
    /// </summary>
    public class HtmlWriter : IHtmlWriter
    {
        private readonly TextWriter writer;

        private OrderedDictionary attributes = new OrderedDictionary();
        private Stack<string> openTags = new Stack<string>();


        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlWriter"/> class.
        /// </summary>
        public HtmlWriter(TextWriter writer)
        {
            this.writer = writer;
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
        public void AddAttribute(string name, string value, bool append = false, string appendSeparator = ";")
        {
            if (append)
            {
                if (attributes.Contains(name))
                {
                    var currentValue = attributes[name] as string;
                    if (!string.IsNullOrWhiteSpace(currentValue))
                    {
                        // append the value with the separator
                        attributes[name] = currentValue + appendSeparator + value;
                        return;
                    }
                }
            }

            // set the value
            attributes[name] = value;
        }

        /// <summary>
        /// Renders the begin tag with attributes that were added in <see cref="AddAttribute"/> method.
        /// </summary>
        public void RenderBeginTag(string name)
        {
            RenderBeginTagCore(name);
            writer.Write(">");
            openTags.Push(name);
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
                    writer.Write(" ");
                    writer.Write(attr.Key as string);
                    writer.Write("=");
                    writer.Write("\"");
                    writer.Write(WebUtility.HtmlEncode(attr.Value as string).Replace("\"", "&quot;"));
                    writer.Write("\"");
                }
            }

            attributes.Clear();
        }

        /// <summary>
        /// Renders the end tag.
        /// </summary>
        public void RenderEndTag()
        {
            if (openTags.Count == 0)
            {
                throw new InvalidOperationException(Parser_RwHtml.HtmlWriter_CannotCloseTagBecauseNoTagIsOpen);
            }

            var name = openTags.Pop();
            writer.Write("</");
            writer.Write(name);
            writer.Write(">");
        }

        /// <summary>
        /// Writes the text.
        /// </summary>
        public void WriteText(string text)
        {
            WebUtility.HtmlEncode(text, writer);
        }

        /// <summary>
        /// Writes the unencoded text.
        /// </summary>
        public void WriteUnencodedText(string text)
        {
            writer.Write(text);
        }
    }
}