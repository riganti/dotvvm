using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Compilation
{
    /// <summary>
    /// Implements common attribute merging strategies - collection concatenation
    /// </summary>
    public class DefaultAttributeValueMerger : AttributeValueMergerBase
    {
        public static T[] MergeValues<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            return Enumerable.Concat(a, b).ToArray();
        }
    }
}
