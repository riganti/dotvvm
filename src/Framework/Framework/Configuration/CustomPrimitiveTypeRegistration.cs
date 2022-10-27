using System;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class CustomPrimitiveTypeRegistration<T> : CustomPrimitiveTypeRegistration
    {

        /// <summary>
        /// Gets a function which can parse the value from string or other type that may appear in route parameters collection.
        /// </summary>
        public Func<object?, T?> ParseValue { get; }

        /// <inheritdoc />
        public override Type ServerSideType => typeof(T);

        public CustomPrimitiveTypeRegistration(Type clientSideType, Func<object?, T?> parseValue, JsonConverter? jsonConverter = null)
            : base(clientSideType, jsonConverter)
        {
            ParseValue = parseValue;
        }

        /// <inheritdoc />
        public override object? ConvertToServerSideType(object? value) => ParseValue(value);
    }

    public abstract class CustomPrimitiveTypeRegistration
    {

        /// <summary>
        /// Gets a JsonConverter which can read and write the client-side representation of the value.
        /// </summary>
        public JsonConverter? JsonConverter { get; }

        /// <summary>
        /// Gets a type which will be used for this custom type on the client side. Only types from ReflectionUtils.PrimitiveTypes (or their nullable versions) are supported.
        /// </summary>
        public Type ClientSideType { get; }

        /// <summary>
        /// Gets a type which appears in viewmodels that is treated as a primitive type by DotVVM.
        /// </summary>
        public abstract Type ServerSideType { get; }

        /// <summary>
        /// Parses the value from string or other type that may appear in route parameters collection.
        /// </summary>
        public abstract object? ConvertToServerSideType(object? value);

        protected CustomPrimitiveTypeRegistration(Type clientSideType, JsonConverter? jsonConverter = null)
        {
            ClientSideType = clientSideType;
            JsonConverter = jsonConverter;
        }

    }
}
