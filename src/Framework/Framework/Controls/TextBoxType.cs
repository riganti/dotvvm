using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    public enum TextBoxType
    {
        /// <summary> The standard <c>&lt;input type=text</c> text box. </summary>
        Normal,
        /// <summary> The <c>&lt;input type=password</c> text box which hides the written text. </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/password" />
        Password,
        /// <summary> The <c>&lt;textarea&gt;</c> element which allows writing multiple lines. </summary>
        MultiLine,
        /// <summary> The <c>&lt;input type=tel</c> text box. </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/tel" />
        Telephone,
        /// <summary> The <c>&lt;input type=url</c> text box which automatically validates whether the user to entered a valid URL. </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/url" />
        Url,
        /// <summary> The <c>&lt;input type=email</c> text box which automatically validates whether the user to entered a valid email address. </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/email" />
        Email,
        /// <summary> The <c>&lt;input type=datetime</c> element which typicaly shows a date picker (without time). </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/date" />
        Date,
        /// <summary> The <c>&lt;input type=datetime-local</c> element which typicaly shows a time-of-day picker (without date). </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/time" />
        Time,
        /// <summary> The <c>&lt;input type=number</c> element which typically shows an interactive color picker and stored its 7-character RGB color code in hexadecimal format into the bound view model property. </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/color" />
        Color,
        /// <summary> The <c>&lt;input type=range</c> text box. </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/search" />
        Search,
        /// <summary> The <c>&lt;input type=range</c> text box which only allows typing digits and typically has up/down arrows. </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/number" />
        Number,
        /// <summary> The <c>&lt;input type=range</c> text box which allows the user to specify a year and month combination in the YYYY-MM format. </summary>
        /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/month" />
        Month,
        /// <summary> The <c>&lt;input type=range</c> text box which allows the user to specify a date time in their local timezone. DotVVM can automatically convert it into a UTC timestamp using the <see cref="DotVVM.Framework.Binding.HelperNamespace.DateTimeExtensions.ToBrowserLocalTime(DateTime)" /> two-way function. </summary>
        DateTimeLocal
    }
}
