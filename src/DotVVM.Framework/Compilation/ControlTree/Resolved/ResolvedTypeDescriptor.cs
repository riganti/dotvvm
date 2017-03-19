using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
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
            return Type.GetTypeInfo().GetCustomAttribute<ControlMarkupOptionsAttribute>();
        }

        public bool IsEqualTo(ITypeDescriptor other)
        {
            return Name == other.Name && Namespace == other.Namespace && Assembly == other.Assembly;
        }

        public ITypeDescriptor TryGetArrayElementOrIEnumerableType()
        {
            // handle array
            if (Type.IsArray)
            {
                return new ResolvedTypeDescriptor(Type.GetElementType());
            }

            // handle iEnumerables
            Type iEnumerable;
            if (Type.GetTypeInfo().IsGenericType && Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                iEnumerable = Type;
            }
            else
            {
                iEnumerable = Type.GetInterfaces()
                    .FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            }
            if (iEnumerable != null)
            {
                return new ResolvedTypeDescriptor(iEnumerable.GetGenericArguments()[0]);
            }

            if (typeof(IEnumerable).IsAssignableFrom(Type))
            {
                return new ResolvedTypeDescriptor(typeof(object));
            }

            return null;
        }

        public ITypeDescriptor TryGetPropertyType(string propertyName)
        {
            var propertyType = Type.GetProperty(propertyName)?.PropertyType;
            if (propertyType != null)
            {
                return new ResolvedTypeDescriptor(propertyType);
            }
            return null;
        }
        
        public override string ToString() => Type.ToString();


        public static Type ToSystemType(ITypeDescriptor typeDescriptor)
        {
            if (typeDescriptor == null) return null;
            else if (typeDescriptor is ResolvedTypeDescriptor)
            {
                return ((ResolvedTypeDescriptor) typeDescriptor).Type;
            }
            else
            {
                return Type.GetType(typeDescriptor.FullName + ", " + typeDescriptor.Assembly);
            }
        }

        public static ITypeDescriptor Create(Type t) => t == null ? null : new ResolvedTypeDescriptor(t);
    }
}