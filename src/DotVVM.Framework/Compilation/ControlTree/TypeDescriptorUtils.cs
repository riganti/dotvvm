using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public static class TypeDescriptorUtils
    {

        public static bool IsPrimitiveTypeDescriptor(this ITypeDescriptor type)
        {
            return type is ResolvedTypeDescriptor resolvedType
                && ReflectionUtils.IsPrimitiveType(resolvedType.Type);
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
            if (type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(IBaseGridViewDataSet<object>))))
            {
                var itemsType = type.TryGetPropertyType("Items");
                return itemsType.TryGetArrayElementOrIEnumerableType() ?? throw new Exception("This is strange and should not happen. IBaseGridViewDataSet.Items is not IEnumerable.");
            }

            throw new NotSupportedException($"The type '{type}' is not a collection or a IBaseGridViewDataSet!");
        }
    }
}
