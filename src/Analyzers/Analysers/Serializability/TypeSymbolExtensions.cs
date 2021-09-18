using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DotVVM.Analysers.Serializability
{
    internal static class TypeSymbolExtensions
    {
        public static bool IsPrimitive(this ITypeSymbol typeSymbol)
        {
            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Byte:
                case SpecialType.System_Char:
                case SpecialType.System_Double:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                case SpecialType.System_SByte:
                case SpecialType.System_Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSerializable(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol.IsPrimitive())
                return true;

            switch (typeSymbol.TypeKind)
            {
                case TypeKind.Array:
                    return IsSerializable(((IArrayTypeSymbol)typeSymbol).ElementType);

                case TypeKind.Enum:
                    return IsSerializable(((INamedTypeSymbol)typeSymbol).EnumUnderlyingType);

                case TypeKind.TypeParameter:
                case TypeKind.Interface:
                    // The concrete type can't be determined statically,
                    // so we assume true to cut down on noise.
                    return true;

                case TypeKind.Class:
                case TypeKind.Struct:
                    // Check SerializableAttribute or Serializable flag from metadata.
                    return ((INamedTypeSymbol)typeSymbol).IsSerializable;

                case TypeKind.Delegate:
                    // delegates are always serializable, even if
                    // they aren't actually marked [Serializable]
                    return true;

                default:
                    return typeSymbol.GetAttributes().Any(a => a.AttributeClass.Name == "SerializableAttribute");
            }
        }
    }
}
