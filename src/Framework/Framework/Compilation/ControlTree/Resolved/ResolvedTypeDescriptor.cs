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
using FastExpressionCompiler;
using Newtonsoft.Json;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    [JsonConverter(typeof(ResourceManagement.DotvvmTypeDescriptorJsonConverter))]
    public sealed class ResolvedTypeDescriptor : ITypeDescriptor
    {
        private static ConcurrentDictionary<(Type, string), ResolvedTypeDescriptor?> cache = new ConcurrentDictionary<(Type, string), ResolvedTypeDescriptor?>();
        public Type Type { get; }

        public ResolvedTypeDescriptor(Type type)
        {
            Type = type;
        }

        public string Name => Type.Name;

        public string? Namespace => Type.Namespace;

        public string? Assembly => Type.Assembly?.FullName;

        public string FullName => Type.FullName ?? (string.IsNullOrEmpty(Namespace) ? Name : (Namespace + "." + Name));

        public string CSharpName => Type.ToCode(stripNamespace: true);
        public string CSharpFullName => Type.ToCode();

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

        public bool IsEqualTo(Type other)
        {
            return this.Type == other;
        }
        public bool IsEqualTo(ITypeDescriptor other)
        {
            if (other is ResolvedTypeDescriptor { Type: var otherType })
                return this.IsEqualTo(otherType);
            return Name == other.Name && Namespace == other.Namespace && Assembly == other.Assembly;
        }

        public override bool Equals(object? obj) =>
            obj is null ? false :
            obj is ResolvedTypeDescriptor { Type: var otherType } ? IsEqualTo(otherType) :
            obj is Type otherType2 ? IsEqualTo(otherType2) :
            obj is ITypeDescriptor typeD ? IsEqualTo(typeD) :
            false;
        public override int GetHashCode() => Type.GetHashCode();

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

        public IEnumerable<ITypeDescriptor> FindGenericImplementations(ITypeDescriptor genericType)
        {
            var generic = (genericType as ResolvedTypeDescriptor)?.Type ?? throw new InvalidOperationException($"Only {nameof(ResolvedTypeDescriptor)} sould be used as a parameter.");
                return ReflectionUtils.GetBaseTypesAndInterfaces(Type)
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == generic)
                .Select(t => new ResolvedTypeDescriptor(t));
        }

        public ITypeDescriptor[]? GetGenericArguments()
        {
            if (!Type.IsGenericType)
            {
                return null;
            }

            return Type.GetGenericArguments()
                .Select(t => new ResolvedTypeDescriptor(t))
                .ToArray();
        }

        public override string ToString() => Type.ToString();


        [return: NotNullIfNotNull("typeDescriptor")]
        public static Type? ToSystemType(ITypeDescriptor? typeDescriptor)
        {
            if (typeDescriptor == null) return null;
            else if (typeDescriptor is ResolvedTypeDescriptor)
            {
                return ((ResolvedTypeDescriptor)typeDescriptor).Type;
            }
            else
            {
                return
                    Type.GetType(typeDescriptor.FullName + ", " + typeDescriptor.Assembly)
                    ?? throw new InvalidOperationException($"Type {typeDescriptor.FullName} could not be found using reflection.");
            }
        }


        [return: NotNullIfNotNull("t")]
        public static ITypeDescriptor? Create(Type? t) => t is null ? null : new ResolvedTypeDescriptor(t);
    }
}
