
using System;


/// <summary> The object will be formatted by the DotVVM error page as HTML rich text </summary>
public interface IDebugHtmlFormattableObject
{
    /// <summary>
    /// Returns text similar to <see cref="object.ToString()"/>, but formatted as HTML.
    /// If the object is exception, only the same information as <see cref="Exception.Message"/> should be returned (no stack trace).
    /// </summary>
    /// <param name="formatProvider">Locale</param>
    /// <param name="isBlock">If true, the text is allowed to be formatted as a block element (i.e. &lt;ul&gt;)</param>
    string DebugHtmlString(IFormatProvider? formatProvider, bool isBlock);
}
