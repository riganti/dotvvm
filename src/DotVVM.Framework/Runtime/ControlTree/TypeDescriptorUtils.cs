using System;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public static class TypeDescriptorUtils
    {

        public static ITypeDescriptor GetCollectionItemType(ITypeDescriptor type)
        {
            // handle IEnumerables
            var iEnumerableType = type.TryGetArrayElementOrIEnumerableType();
            if (iEnumerableType != null)
            {
                return iEnumerableType;
            }
            
            // handle GridViewDataSet
            if (type.IsAssignableTo(new ResolvedTypeDescriptor(typeof(IGridViewDataSet))))
            {
                var itemsType = type.TryGetPropertyType(nameof(IGridViewDataSet.Items));
                return itemsType.TryGetArrayElementOrIEnumerableType();
            }

            throw new NotSupportedException($"The type '{type}' is not a collection or a IGridViewDataSet!");
        }


    }
}