using System;

namespace DotVVM.Framework.Configuration
{
    public sealed class CustomPrimitiveTypeRegistration
    {
        private readonly Func<object?, object?> convertFunction;

        public Type ClientSideType { get; }

        public Type ServerSideType { get; }
        
        internal CustomPrimitiveTypeRegistration(Type serverSideType, Type clientSideType, Func<object?, object?> convertFunction)
        {
            ServerSideType = serverSideType;
            ClientSideType = clientSideType;
            this.convertFunction = convertFunction;
        }

        public object? ConvertToServerSideType(object? value) => convertFunction(value);

    }
}
