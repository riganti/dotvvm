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
        public static DataContextStack CreateChildStack(this DotvvmBindableObject bindableObject, Type viewModelType)
        {
            var dataContextTypeStack = bindableObject.GetDataContextType();

            return DataContextStack.Create(
                viewModelType, 
                dataContextTypeStack,
                dataContextTypeStack.NamespaceImports,
                dataContextTypeStack.ExtensionParameters);
        }
    }
}