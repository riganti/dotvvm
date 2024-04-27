using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public class DataPagerApi
    {
        public void Load() => throw new NotSupportedException("The _dataPager.Load method is not supported on the server, please use a staticCommand to invoke it.");

        public bool IsLoading => false;

        public bool CanLoadNextPage => true;


        public class DataPagerExtensionParameter : BindingExtensionParameter
        {
            public DataPagerExtensionParameter(string identifier, bool inherit = true) : base(identifier, ResolvedTypeDescriptor.Create(typeof(DataPagerApi)), inherit)
            {
            }

            internal static void Register(JavascriptTranslatableMethodCollection collection)
            {
                collection.AddMethodTranslator(() => default(DataPagerApi)!.Load(), new GenericMethodCompiler(args =>
                    args[0].Member("$appendableDataPager").Member("loadNextPage").Invoke().WithAnnotation(new ResultIsPromiseAnnotation(e => e))));

                collection.AddPropertyGetterTranslator(typeof(DataPagerApi), nameof(IsLoading), new GenericMethodCompiler(args =>
                    args[0].Member("$appendableDataPager").Member("isLoading").WithAnnotation(ResultIsObservableAnnotation.Instance)));

                collection.AddPropertyGetterTranslator(typeof(DataPagerApi), nameof(CanLoadNextPage), new GenericMethodCompiler(args =>
                    args[0].Member("$appendableDataPager").Member("canLoadNextPage").WithAnnotation(ResultIsObservableAnnotation.Instance)));
            }

            public override JsExpression GetJsTranslation(JsExpression dataContext) =>
                dataContext;
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

            public override bool NestDataContext => false;
            public override IEnumerable<BindingExtensionParameter> GetExtensionParameters(ITypeDescriptor dataContext)
            {
                return new BindingExtensionParameter[] {
                    new DataPagerExtensionParameter(Name)
                };
            }
        }
    }
}
