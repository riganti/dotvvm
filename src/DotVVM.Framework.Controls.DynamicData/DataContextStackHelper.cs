using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls.DynamicData
{
    public static class DataContextStackHelper
    {
        public static DataContextStack CreateDataContextStack(DotvvmControl control)
        {
            //TODO: Will parameters be accesible? Where to get them from?
            //Namespace imports appear to be incorectly set as it is
            return (new[] { control }).Concat(control.GetAllAncestors())
                .Reverse()
                .Where(c => c.IsPropertySet(DotvvmBindableObject.DataContextProperty, false))
                .Select(GetDataContextType)
                .Aggregate((DataContextStack)null, (parent, type) => DataContextStack.Create(type, parent, new List<NamespaceImport>()));
        }

        private static Type GetDataContextType(DotvvmBindableObject control)
        {
            return control.GetValueBinding(DotvvmBindableObject.DataContextProperty, false)
                .As<ValueBindingExpression>()
                ?.ResultType 
                ?? control.DataContext.GetType();
        }

        public static DataContextStack CreateChildStack(Type entityType, DataContextStack dataContextStack)
        {
            return DataContextStack.Create(entityType, dataContextStack, dataContextStack.NamespaceImports, dataContextStack.ExtensionParameters);
        }
    }
}