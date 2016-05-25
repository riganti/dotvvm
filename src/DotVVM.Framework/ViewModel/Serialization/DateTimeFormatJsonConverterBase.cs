using System;
using System.Globalization;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// A converter that helps with using DateTime on server side and string on client side.
    /// </summary>
    public abstract class DateTimeFormatJsonConverterBase : FormatJsonConverterBase<DateTime>
    {

        /// <summary>
        /// Gets an array of allowed format strings. The first one will be used when the value is serialized and sent to the client.
        /// </summary>
        public abstract string[] DateTimeFormats { get; }

        /// <summary>
        /// Gets the format provider that will perform the formatting. 
        /// </summary>
        public virtual IFormatProvider FormatProvider => CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets the date time style flags.
        /// </summary>
        public virtual DateTimeStyles DateTimeStyles => DateTimeStyles.None;

        /// <summary>
        /// Tries the convert the value from string and returns whether the attempt was successful.
        /// </summary>
        /// <param name="value">A value to convert.</param>
        /// <param name="result">The result value in case the conversion was successful.</param>
        protected override bool TryConvertFromString(string value, out DateTime result)
        {
            return DateTime.TryParseExact(value, DateTimeFormats, FormatProvider, DateTimeStyles, out result);
        }

        /// <summary>
        /// Provides a value that should be used when the value couldn't be parsed from string.
        /// </summary>
        protected override DateTime? ProvideEmptyValue(Type objectType)
        {
            return objectType == typeof(DateTime?) ? (DateTime?)null : DateTime.MinValue;
        }

        /// <summary>
        /// Converts the value to a string representation before sending it to the client.
        /// </summary>
        protected override string ConvertToString(object value)
        {
            return ((DateTime)value).ToString(DateTimeFormats[0], FormatProvider);
        }
    }
}