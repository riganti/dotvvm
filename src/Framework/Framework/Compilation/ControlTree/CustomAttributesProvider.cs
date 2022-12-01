using System;
using System.Linq;
using System.Reflection;

namespace DotVVM.Framework.Compilation.ControlTree
{
    internal class CustomAttributesProvider : ICustomAttributeProvider
    {
        private readonly object[] attributes;
        public CustomAttributesProvider(params object[] attributes)
        {
            this.attributes = attributes;
        }
        public object[] GetCustomAttributes(bool inherit) => attributes;

        public object[] GetCustomAttributes(Type attributeType, bool inherit) => GetCustomAttributes(inherit).Where(attributeType.IsInstanceOfType).ToArray();

        public bool IsDefined(Type attributeType, bool inherit) => GetCustomAttributes(attributeType, inherit).Any();
    }
}
