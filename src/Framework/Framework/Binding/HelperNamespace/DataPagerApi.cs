using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Core.Storage;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public class DataPagerApi
    {
        public void Load() => throw new NotSupportedException("The _dataPager.Load method is not supported on the server, please use a staticCommand to invoke it.");




        public class DataPagerExtensionParameter : BindingExtensionParameter
        {
            public DataPagerExtensionParameter(string identifier, bool inherit = true) : base(identifier, ResolvedTypeDescriptor.Create(typeof(DataPagerApi)), inherit)
            {
            }

            public override JsExpression GetJsTranslation(JsExpression dataContext) =>
                new JsObjectExpression();
            public override Expression GetServerEquivalent(Expression controlParameter) =>
                Expression.New(typeof(DataPagerApi));
        }

        public class AddParameterDataContextChangeAttribute: DataContextChangeAttribute
        {
            public AddParameterDataContextChangeAttribute(string name = "_dataPager", int order = 0)
            {
                Name = name;
                Order = order;
            }

            public string Name { get; }
            public override int Order { get; }

            public override ITypeDescriptor? GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor? property = null) =>
                dataContext;
            public override Type? GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, DotvvmBindableObject control, DotvvmProperty? property = null) => dataContext;

            public override IEnumerable<BindingExtensionParameter> GetExtensionParameters(ITypeDescriptor dataContext)
            {
                return new BindingExtensionParameter[] {
                    new DataPagerExtensionParameter(Name)
                };
            }
        }
    }
}
