using System;
using System.Reflection;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Runtime.ControlTree.Resolved
{
    public class ResolvedTypeDescriptor : ITypeDescriptor
    {
        public Type Type { get; }

        public ResolvedTypeDescriptor(Type type)
        {
            Type = type;
        }

        public string Name => Type.Name;
        public string Namespace => Type.Namespace;
        public string Assembly => Type.AssemblyQualifiedName;
        public string FullName => string.IsNullOrEmpty(Namespace) ? Name : (Namespace + "." + Name);

        public bool IsAssignableTo(ITypeDescriptor typeDescriptor)
        {
            return ToSystemType(typeDescriptor).IsAssignableFrom(Type);
        }

        public bool IsAssignableFrom(ITypeDescriptor typeDescriptor)
        {
            return Type.IsAssignableFrom(ToSystemType(typeDescriptor));
        }

        public ControlMarkupOptionsAttribute GetControlMarkupOptionsAttribute()
        {
            return Type.GetCustomAttribute<ControlMarkupOptionsAttribute>();
        }

        public bool IsEqualTo(ITypeDescriptor other)
        {
            return Name == other.Name && Namespace == other.Namespace && Assembly == other.Assembly;
        }


        public static Type ToSystemType(ITypeDescriptor typeDescriptor)
        {
            if (typeDescriptor is ResolvedTypeDescriptor)
            {
                return ((ResolvedTypeDescriptor) typeDescriptor).Type;
            }
            else
            {
                return Type.GetType(typeDescriptor.FullName + ", " + typeDescriptor.Assembly);
            }
        }
    }
}