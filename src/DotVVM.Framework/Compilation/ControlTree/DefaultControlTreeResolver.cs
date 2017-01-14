using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree
{
    /// <summary>
    /// A runtime implementation of the control tree resolver.
    /// </summary>
    public class DefaultControlTreeResolver : ControlTreeResolverBase
    {
        private readonly IBindingExpressionBuilder bindingExpressionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultControlTreeResolver"/> class.
        /// </summary>
        public DefaultControlTreeResolver(DotvvmConfiguration configuration)
            : base(configuration.ServiceLocator.GetService<IControlResolver>(), configuration.ServiceLocator.GetService<IAbstractTreeBuilder>())
        {
            this.bindingExpressionBuilder = configuration.ServiceLocator.GetService<IBindingExpressionBuilder>();
        }

        protected override IControlType CreateControlType(ITypeDescriptor wrapperType, string virtualPath)
        {
            return new ControlType(ResolvedTypeDescriptor.ToSystemType(wrapperType), virtualPath: virtualPath);
        }

        protected override IDataContextStack CreateDataContextTypeStack(ITypeDescriptor viewModelType, ITypeDescriptor wrapperType = null, IDataContextStack parentDataContextStack = null,  IReadOnlyList<NamespaceImport> namespaceImports = null)
        {
            return new DataContextStack(
                ResolvedTypeDescriptor.ToSystemType(viewModelType),
                parentDataContextStack as DataContextStack,
                ResolvedTypeDescriptor.ToSystemType(wrapperType), namespaceImports);
        }

        protected override IAbstractBinding CompileBinding(DothtmlBindingNode node, BindingParserOptions bindingOptions, IDataContextStack context, IPropertyDescriptor property)
        {
            if (context == null)
            {
                node.AddError("The DataContext couldn't be evaluated because of the errors above.");
            }
            return treeBuilder.BuildBinding(bindingOptions, context, node, property);
        }

        protected override object ConvertValue(string value, ITypeDescriptor propertyType)
        {
            return ReflectionUtils.ConvertValue(value, ((ResolvedTypeDescriptor)propertyType).Type);
        }
    }
}