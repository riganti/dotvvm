using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DotVVM.Framework.Configuration
{
    public struct HtmlTagAttributePair : IEquatable<HtmlTagAttributePair>
    {

        public string TagName { get; set; }

        public string AttributeName { get; set; }

        public HtmlTagAttributePair(string tagName, string attributeName)
        {
            TagName = tagName;
            AttributeName = attributeName;
        }

        public override string ToString()
        {
            return TagName + "[" + AttributeName + "]";
        }

        public bool Equals(HtmlTagAttributePair other)
        {
            return string.Equals(TagName, other.TagName, StringComparison.OrdinalIgnoreCase) && string.Equals(AttributeName, other.AttributeName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((TagName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(TagName) : 0) * 397) ^ (AttributeName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(AttributeName) : 0);
            }
        }
        public override bool Equals(object? obj)
        {
            if (obj is HtmlTagAttributePair o)
            {
                return Equals(o);
            }
            return false;
        }
    }

    public class HtmlTagAttributePairToStringConverter : JsonConverter<HtmlTagAttributePair>
    {
        public override void Write(Utf8JsonWriter writer, HtmlTagAttributePair value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }

        public override HtmlTagAttributePair Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null)
            {
                throw InvalidFormatException();
            }

            var match = Regex.Match(value, @"^([a-zA-Z0-9]+)\[([a-zA-Z0-9]+)\]$", RegexOptions.CultureInvariant);
            if (!match.Success)
            {
                throw InvalidFormatException();
            }
            return new() {
                TagName = match.Groups[1].Value,
                AttributeName = match.Groups[2].Value
            };
        }

        private static Exception InvalidFormatException() =>
            new Exception("HTML attribute definition expected! Correct syntax is 'a[href]': { }");

    }
}
