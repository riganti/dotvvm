using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public static class TypeDescriptorUtils
    {
        private static readonly HashSet<ResolvedTypeDescriptor> NumericTypeDescriptors = new HashSet<ResolvedTypeDescriptor>()
     {
             new ResolvedTypeDescriptor(typeof(sbyte)),
             new ResolvedTypeDescriptor(typeof(byte)),
             new ResolvedTypeDescriptor(typeof(short)),
             new ResolvedTypeDescriptor(typeof(ushort)),
             new ResolvedTypeDescriptor(typeof(int)),
             new ResolvedTypeDescriptor(typeof(uint)),
             new ResolvedTypeDescriptor(typeof(long)),
             new ResolvedTypeDescriptor(typeof(ulong)),
             new ResolvedTypeDescriptor(typeof(char)),
             new ResolvedTypeDescriptor(typeof(float)),
             new ResolvedTypeDescriptor(typeof(double)),
             new ResolvedTypeDescriptor(typeof(decimal))
        };
        private static readonly ResolvedTypeDescriptor StringTypeDescriptor = new ResolvedTypeDescriptor(typeof(string));
        private static readonly ResolvedTypeDescriptor EnumTypeDescriptor = new ResolvedTypeDescriptor(typeof(Enum));

        public static bool IsNumericTypeDescriptor(this ITypeDescriptor type)
        {
            return NumericTypeDescriptors.Any(a => a.IsEqualTo(type));
        }

        public static bool IsEnumTypeDescriptor(this ITypeDescriptor type)
        {
            return type.IsAssignableTo(EnumTypeDescriptor);
        }
        public static bool IsStringTypeDescriptor(this ITypeDescriptor type)
        {
            return StringTypeDescriptor.IsEqualTo(type);
        }

        public static bool IsPrimitiveTypeDescriptor(this ITypeDescriptor type)
        {
            return type.IsStringTypeDescriptor() || type.IsNumericTypeDescriptor();
        }
        public static bool IsPrimitiveAndEnumTypeDescriptor(this ITypeDescriptor type)
        {
            return type.IsPrimitiveTypeDescriptor() || type.IsEnumTypeDescriptor();
        }
        public static ITypeDescriptor GetCollectionItemType(ITypeDescriptor type)
        {
            // handle IEnumerables
            var iEnumerableType = type.TryGetArrayElementOrIEnumerableType();
            if (iEnumerableType != null)
            {
                return iEnumerableType;
            }

            // handle GridViewDataSet
            if (type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(IBaseGridViewDataSet))))
            {
                var itemsType = type.TryGetPropertyType(nameof(IBaseGridViewDataSet.Items));
                return itemsType.TryGetArrayElementOrIEnumerableType() ?? throw new Exception("This is strange and should not happen. IBaseGridViewDataSet.Items is not IEnumerable.");
            }

            throw new NotSupportedException($"The type '{type}' is not a collection or a IBaseGridViewDataSet!");
        }
    }
}
