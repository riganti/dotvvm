using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotVVM.Framework.Utils
{
    internal static class TypeHashSerializer
    {
        public static byte[] Serialize(Type type)
        {
            const byte Version = 1;
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write(Version);
                WriteType(writer, type);
            }
            return stream.ToArray();
        }

        private static void WriteType(BinaryWriter writer, Type type)
        {
            const byte ByRef = 1;
            const byte Pointer = 2;
            const byte Array = 3;
            const byte SZArray = 4;
            const byte GenericParameter = 5;
            const byte ConstructedGeneric = 6;
            const byte Named = 7;
            if (type.IsByRef)
            {
                writer.Write(ByRef);
                WriteType(writer, type.GetElementType()!);
            }
            else if (type.IsPointer)
            {
                writer.Write(Pointer);
                WriteType(writer, type.GetElementType()!);
            }
            else if (type.IsArray)
            {
                writer.Write(IsSZArrayType(type) ? SZArray : Array);
                writer.Write(type.GetArrayRank());
                WriteType(writer, type.GetElementType()!);
            }
            else if (type.IsGenericParameter)
            {
                writer.Write(GenericParameter);
                writer.Write(type.DeclaringMethod is {});
                writer.Write(type.GenericParameterPosition);
                writer.Write(type.Name);
            }
            else if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                writer.Write(ConstructedGeneric);
                WriteNamedTypeIdentity(writer, type.GetGenericTypeDefinition());

                var genericArguments = type.GetGenericArguments();
                writer.Write(genericArguments.Length);
                foreach (var genericArgument in genericArguments)
                {
                    WriteType(writer, genericArgument);
                }
            }
            else
            {
                writer.Write(Named);
                WriteNamedTypeIdentity(writer, type);
            }
        }

        private static bool IsSZArrayType(Type type)
        {
#if DotNetCore
            return type.IsSZArray;
#else
            var elementType = type.GetElementType();
            return elementType != null && type == elementType.MakeArrayType();
#endif
        }

        private static void WriteNamedTypeIdentity(BinaryWriter writer, Type type)
        {
            var assemblyName = type.Assembly.GetName().Name ?? "";
            if (assemblyName == "mscorlib")
            {
                // Keep hashes for basic types stable between .NET Framework and .NET Core.
                assemblyName = "System.Private.CoreLib";
            }

            writer.Write(assemblyName);
            writer.Write(type.Namespace ?? "");

            var typeHierarchy = new List<Type>();
            for (var currentType = type; currentType is {}; currentType = currentType.DeclaringType)
            {
                typeHierarchy.Add(currentType);
            }

            writer.Write(typeHierarchy.Count);
            foreach (var hierarchyType in typeHierarchy)
            {
                writer.Write(hierarchyType.Name);
            }
        }
    }
}
