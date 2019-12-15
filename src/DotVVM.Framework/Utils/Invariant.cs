using System.Globalization;

namespace DotVVM.Framework.Utils
{
    /// <summary>
    /// Provides convenient methods for quickly parsing values from strings rendered in the
    /// invariant culture.
    /// </summary>
    public static class Invariant
    {
        /// <summary>
        /// Tries to parse a decimal number out of a string using the <see cref="CultureInfo.InvariantCulture" />.
        /// The method accepts null strings and leading and trailing whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParse(string str, out decimal value)
            => decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a double-precision floating point number out of a string using the
        /// <see cref="CultureInfo.InvariantCulture" />. The method accepts null strings and leading
        /// and trailing whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParse(string str, out double value)
            => double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a single-precision floating point number out of a string using the
        /// <see cref="CultureInfo.InvariantCulture" />. The method accepts null strings and leading
        /// and trailing whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParse(string str, out float value)
            => float.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a 32-bit signed integer out of a string using the same format as the
        /// <see cref="CultureInfo.InvariantCulture" />. The method accepts null strings and leading
        /// and trailing whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParse(string str, out int value)
            => int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a 32-bit unsigned integer out of a string using the same format as the
        /// <see cref="CultureInfo.InvariantCulture" />. The method accepts null strings and leading
        /// and trailing whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        /// <returns></returns>
        public static bool TryParse(string str, out uint value)
            => uint.TryParse(str, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a 64-bit signed integer out of a string using the same format as the
        /// <see cref="CultureInfo.InvariantCulture" />. The method accepts null strings and leading
        /// and trailing whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParse(string str, out long value)
            => long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a 64-bit unsigned integer out of a string using the same format as the
        /// <see cref="CultureInfo.InvariantCulture" />. The method accepts null strings and leading
        /// and trailing whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParse(string str, out ulong value)
            => ulong.TryParse(str, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a double-precision floating point number out of a string using the same
        /// format as the <see cref="CultureInfo.InvariantCulture" />. The method accepts null
        /// strings but does not accept whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParseExact(string str, out double value)
            => double.TryParse(str, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a single-precision floating point number out of a string using the same
        /// format as the <see cref="CultureInfo.InvariantCulture" />. The method accepts null
        /// strings but does not accept whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParseExact(string str, out float value)
            => float.TryParse(str, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a 32-bit signed integer out of a string using the same format as the
        /// <see cref="CultureInfo.InvariantCulture" />. The method accepts null strings but does
        /// not accept whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParseExact(string str, out int value)
            => int.TryParse(str, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a 32-bit unsigned integer out of a string using the same format as the
        /// <see cref="CultureInfo.InvariantCulture" />. The method accepts null strings but does
        /// not accept whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        /// <returns></returns>
        public static bool TryParseExact(string str, out uint value)
            => uint.TryParse(str, NumberStyles.None, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a 64-bit signed integer out of a string using the same format as the
        /// <see cref="CultureInfo.InvariantCulture" />. The method accepts null strings but does
        /// not accept whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParseExact(string str, out long value)
            => long.TryParse(str, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Tries to parse a 64-bit unsigned integer out of a string using the same format as the
        /// <see cref="CultureInfo.InvariantCulture" />. The method accepts null strings but does
        /// not accept whitespace.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="value">The parsed value.</param>
        public static bool TryParseExact(string str, out ulong value)
            => ulong.TryParse(str, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }
}