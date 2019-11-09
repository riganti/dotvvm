#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    public interface IHtmlWriter
    {
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
        void AddAttribute(string name, string? value, bool append = false, string? appendSeparator = null);

        /// <summary>
        /// Adds the style attribute.
        /// </summary>
        /// <param name="name">The name of the CSS property.</param>
        /// <param name="value">The value of the CSS property.</param>
        void AddStyleAttribute(string name, string value);

        /// <summary>
        /// Adds the data-bind attribute to the next HTML element that is being rendered.
        /// </summary>
        /// <param name="name">The name of the binding handler.</param>
        /// <param name="expression">The binding expression.</param>
        void AddKnockoutDataBind(string name, string expression);

        /// <summary>
        /// Adds the data-bind attribute to the next HTML element that is being rendered.
        /// </summary>
        /// <param name="name">The name of the binding handler.</param>
        /// <param name="bindingGroup">A group of name-value pairs.</param>
        void AddKnockoutDataBind(string name, KnockoutBindingGroup bindingGroup);

        /// <summary>
        /// Renders the begin tag with attributes that were added in <see cref="HtmlWriter.AddAttribute"/> method.
        /// </summary>
        void RenderBeginTag(string name);

        /// <summary>
        /// Renders the self closing tag with attributes that were added in <see cref="HtmlWriter.AddAttribute"/> method.
        /// </summary>
        void RenderSelfClosingTag(string name);

        /// <summary>
        /// Renders the end tag.
        /// </summary>
        void RenderEndTag();

        /// <summary>
        /// Writes the text.
        /// </summary>
        void WriteText(string text);

        /// <summary>
        /// Writes the unencoded text.
        /// </summary>
        void WriteUnencodedText(string text);

        /// <summary>
        /// Writes the specified HTML attribute and value (e.g. href="myUrl"). 
        /// This method is typically used from <see cref="IHtmlAttributeTransformer"/> implementations.
        /// </summary>
        void WriteHtmlAttribute(string attributeName, string? attributeValue);

        /// <summary>
        /// Passes information about the currently rendered control to the HtmlWriter. Used only to provide more accurate error/warning/debug information, does not alter the main behavior.
        /// </summary>
        void SetErrorContext(DotvvmBindableObject obj);
    }
}
