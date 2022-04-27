using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedTypeDescriptor : ITypeDescriptor
    {
        private static ConcurrentDictionary<(Type, string), ResolvedTypeDescriptor?> cache = new ConcurrentDictionary<(Type, string), ResolvedTypeDescriptor?>();
        public Type Type { get; }

        public ResolvedTypeDescriptor(Type type)
        {
            Type = type;
        }

        public string Name => Type.Name;

        public string? Namespace => Type.Namespace;

        public string? Assembly => Type.AssemblyQualifiedName;

        public string FullName => string.IsNullOrEmpty(Namespace) ? Name : (Namespace + "." + Name);

        public bool IsAssignableTo(ITypeDescriptor typeDescriptor)
        {
            return ToSystemType(typeDescriptor).IsAssignableFrom(Type);
        }

        public bool IsAssignableFrom(ITypeDescriptor typeDescriptor)
        {
            return Type.IsAssignableFrom(ToSystemType(typeDescriptor));
        }

        public ControlMarkupOptionsAttribute? GetControlMarkupOptionsAttribute()
        {
            return Type.GetCustomAttribute<ControlMarkupOptionsAttribute>();
        }

        public bool IsEqualTo(ITypeDescriptor other)
        {
            return Name == other.Name && Namespace == other.Namespace && Assembly == other.Assembly;
        }

        public ITypeDescriptor? TryGetArrayElementOrIEnumerableType()
        {
            // handle array
            if (Type.IsArray)
            {
                return new ResolvedTypeDescriptor(Type.GetElementType()!);
            }

            // handle iEnumerables
            Type? iEnumerable;
            if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                iEnumerable = Type;
            }
            else
            {
                iEnumerable = Type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
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

        public ITypeDescriptor? TryGetPropertyType(string propertyName)
        {
            return cache.GetOrAdd((Type, propertyName), type => {
                if (!Type.IsInterface)
                {
                    var propertyType = Type.GetProperty(propertyName)?.PropertyType;
                    if (propertyType != null)
                    {
                        return new ResolvedTypeDescriptor(propertyType);
                    }
                }
                else
                {
                    var candidates = Type.GetInterfaces().Where(s => s.GetProperty(propertyName) != null).ToList();
                    // this is not nice and I don't like it but the problem is shadowing of props in interfaces.
                    var propertyType = candidates
                        .First(s => candidates.Where(c => c != s).All(b => b.GetInterfaces().All(n => n != s)))
                        .GetProperty(propertyName)?.PropertyType;
                    if (propertyType != null)
                    {
                        return new ResolvedTypeDescriptor(propertyType);
                    }

                }
                return null;
            });
        }

        public ITypeDescriptor MakeGenericType(params ITypeDescriptor[] typeArguments)
        {
            var genericType = Type.MakeGenericType(typeArguments
                .Cast<ResolvedTypeDescriptor>().Select(t => t.Type)
                .ToArray());

            return new ResolvedTypeDescriptor(genericType);
        }

        public override string ToString() => Type.ToString();


        [return: NotNullIfNotNull("typeDescriptor")]
        public static Type? ToSystemType(ITypeDescriptor? typeDescriptor)
        {
            if (typeDescriptor == null) return null;
            
            return TryToSystemType(typeDescriptor) ??
                    throw new InvalidOperationException($"Type {typeDescriptor.FullName}, {typeDescriptor.Assembly} could not be found using reflection.");
        }

        public static Type? TryToSystemType(ITypeDescriptor? typeDescriptor)
        {
            if (typeDescriptor == null) return null;
            else if (typeDescriptor is ResolvedTypeDescriptor { Type: var type })
            {
                return type;
            }
            else
            {
                return
                    Type.GetType(typeDescriptor.FullName + ", " + typeDescriptor.Assembly) ??
                    Type.GetType(typeDescriptor.FullName);
            }
        }


        [return: NotNullIfNotNull("t")]
        public static ITypeDescriptor? Create(Type? t) => t is null ? null : new ResolvedTypeDescriptor(t);
    }
}
