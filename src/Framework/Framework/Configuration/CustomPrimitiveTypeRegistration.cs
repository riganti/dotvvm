using System;

namespace DotVVM.Framework.Configuration
{
    public class CustomPrimitiveTypeRegistration
    {
        // TODO: think about better name
        public Type Type { get; }

        public Type ClientSidePrimitiveType { get; }

        public CustomPrimitiveTypeRegistration(Type type, Type clientSidePrimitiveType)
        {
            Type = type;
            ClientSidePrimitiveType = clientSidePrimitiveType;
        }
    }
}
