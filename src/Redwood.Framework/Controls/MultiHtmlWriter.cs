using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Renders the HTML to multiple HTML writers.
    /// </summary>
    public class MultiHtmlWriter : IHtmlWriter
    {
        private readonly IHtmlWriter[] writers;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiHtmlWriter"/> class.
        /// </summary>
        public MultiHtmlWriter(params IHtmlWriter[] writers)
        {
            this.writers = writers;
        }

        /// <summary>
        /// Adds the specified attribute to the next HTML element that is being rendered.
        /// </summary>
        /// <param name="name">The name of the HTML attribute.</param>
        /// <param name="value">The value of the HTML attribute.</param>
        /// <param name="append">If set to false, the value of the attribute will be overwritten.
        /// If set to true, the value will be appended to the current attribute value and the <paramref name="appendSeparator" /> will be added when needed.</param>
        /// <param name="appendSeparator">The separator that will be used when <paramref name="append" /> is true and when the attribute already has a value.</param>
        public void AddAttribute(string name, string value, bool append = false, string appendSeparator = ";")
        {
            foreach (var writer in writers)
            {
                writer.AddAttribute(name, value, append, appendSeparator);    
            }
        }

        /// <summary>
        /// Adds the style attribute.
        /// </summary>
        /// <param name="name">The name of the CSS property.</param>
        /// <param name="value">The value of the CSS property.</param>
        public void AddStyleAttribute(string name, string value)
        {
            foreach (var writer in writers)
            {
                writer.AddStyleAttribute(name, value);
            }
        }

        /// <summary>
        /// Renders the begin tag with attributes that were added in <see cref="HtmlWriter.AddAttribute" /> method.
        /// </summary>
        /// <param name="name"></param>
        public void RenderBeginTag(string name)
        {
            foreach (var writer in writers)
            {
                writer.RenderBeginTag(name);
            }
        }

        /// <summary>
        /// Renders the self closing tag with attributes that were added in <see cref="HtmlWriter.AddAttribute" /> method.
        /// </summary>
        public void RenderSelfClosingTag(string name)
        {
            foreach (var writer in writers)
            {
                writer.RenderSelfClosingTag(name);
            }
        }

        /// <summary>
        /// Renders the end tag.
        /// </summary>
        public void RenderEndTag()
        {
            foreach (var writer in writers)
            {
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Writes the text.
        /// </summary>
        public void WriteText(string text)
        {
            foreach (var writer in writers)
            {
                writer.WriteText(text);
            }
        }

        /// <summary>
        /// Writes the unencoded text.
        /// </summary>
        public void WriteUnencodedText(string text)
        {
            foreach (var writer in writers)
            {
                writer.WriteUnencodedText(text);
            }
        }
    }
}