using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public struct HtmlTagAttributePair : IEquatable<HtmlTagAttributePair>
    {

        public string TagName { get; set; }

        public string AttributeName { get; set; }

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

    public class HtmlTagAttributePairToStringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.ReadAsString();
            if (value == null)
            {
                throw InvalidFormatException();
            }

            var match = Regex.Match(value, @"^([a-zA-Z0-9]+)\[([a-zA-Z0-9]+)\]$", RegexOptions.CultureInvariant);
            if (!match.Success)
            {
                throw InvalidFormatException();
            }

            if (existingValue == null)
            {
                existingValue = new HtmlTagAttributePair();
            }
            var pair = (HtmlTagAttributePair) existingValue;
            pair.TagName = match.Groups[1].Value;
            pair.AttributeName = match.Groups[2].Value;
            return pair;
        }

        private static Exception InvalidFormatException() =>
            new JsonSerializationException("HTML attribute definition expected! Correct syntax is 'a[href]': { }");

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (HtmlTagAttributePair);
        }
    }
}
