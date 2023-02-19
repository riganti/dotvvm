using System;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Configuration
{
    public sealed class CustomPrimitiveTypeRegistration
    {
        private readonly Func<object?, object?> convertToServerSideType;
        private readonly Func<object?, object?> convertToClientSideType;

        public Type ClientSideType { get; }

        public Type ServerSideType { get; }

        internal CustomPrimitiveTypeRegistration(Type serverSideType, Type clientSideType, Func<object?, object?> convertToServerSideType, Func<object?, object?> convertToClientSideType)
        {
            ServerSideType = serverSideType;
            ClientSideType = clientSideType;
            this.convertToServerSideType = convertToServerSideType;
            this.convertToClientSideType = convertToClientSideType;
        }

        public object? ConvertToServerSideType(object? value)
        {
            var result = convertToServerSideType(value);
            if (result != null && !ServerSideType.IsAssignableFrom(result.GetType()))
            {
                throw new Exception($"The {nameof(ICustomPrimitiveTypeConverter.ToCustomPrimitiveType)} for type {ServerSideType} returned an incompatible type {result?.GetType()}! Expected type: {ServerSideType}");
            }
            return result;
        }

        public object? ConvertToClientSideType(object? value)
        {
            var result = convertToClientSideType(value);
            if (result != null && !ClientSideType.IsAssignableFrom(result.GetType()))
            {
                throw new Exception($"The {nameof(ICustomPrimitiveTypeConverter.FromCustomPrimitiveType)} for type {ServerSideType} returned an incompatible type {result?.GetType()}! Expected type: {ClientSideType}");
            }
            return result;
        }

    }
}
