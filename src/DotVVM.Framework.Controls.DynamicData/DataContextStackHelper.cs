using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Controls.DynamicData
{
    public static class DataContextStackHelper
    {
        public static DataContextStack CreateDataContextStack(DotvvmControl control)
        {
            var rootType = control.GetRoot().GetType();

            return new[] { control }.Concat(control.GetAllAncestors())
                .Reverse()
                .Where(c => c.IsPropertySet(DotvvmBindableObject.DataContextProperty, false))
                .Select(c => (c.GetValueBinding(DotvvmBindableObject.DataContextProperty, false) as ValueBindingExpression)?.ExpressionTree.Type ?? c.DataContext.GetType())
                .Aggregate((DataContextStack)null, (parent, type) => new DataContextStack(type, parent, rootType, new List<NamespaceImport>()));
        }

        public static DataContextStack CreateChildStack(Type entityType, DataContextStack dataContextStack)
        {
            return new DataContextStack(entityType, dataContextStack, dataContextStack.RootControlType, new List<NamespaceImport>());
        }
    }
}