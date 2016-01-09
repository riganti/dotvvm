using System;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.ControlTree;
using DotVVM.Framework.Runtime.ControlTree.Resolved;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Compilation
{
    /// <summary>
    /// A runtime implementation of the control tree resolver.
    /// </summary>
    public class DefaultControlTreeResolver : ControlTreeResolverBase
    {
        private readonly IBindingParser bindingParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultControlTreeResolver"/> class.
        /// </summary>
        public DefaultControlTreeResolver(DotvvmConfiguration configuration) 
            : base(configuration.ServiceLocator.GetService<IControlResolver>(), configuration.ServiceLocator.GetService<IAbstractTreeBuilder>())
        {
            this.bindingParser = configuration.ServiceLocator.GetService<IBindingParser>();
        }

        protected override IControlType CreateControlType(ITypeDescriptor wrapperType, string virtualPath)
        {
            return new ControlType(ResolvedTypeDescriptor.ToSystemType(wrapperType), virtualPath: virtualPath);
        }

        protected override IDataContextStack CreateDataContextTypeStack(ITypeDescriptor viewModelType, ITypeDescriptor wrapperType = null, IDataContextStack parentDataContextStack = null)
        {
            DataContextStack parent = null;
            if (parentDataContextStack != null)
            {
                parent = (DataContextStack) CreateDataContextTypeStack(parentDataContextStack.DataContextType,  
                    parentDataContextStack: parentDataContextStack.Parent);
            }

            var dataContextTypeStack = new DataContextStack(ResolvedTypeDescriptor.ToSystemType(viewModelType), parent);
            if (wrapperType != null)
            {
                dataContextTypeStack.RootControlType = ResolvedTypeDescriptor.ToSystemType(wrapperType);
            }
            return dataContextTypeStack;
        }

        protected override IAbstractBinding CompileBinding(DothtmlBindingNode node, BindingParserOptions bindingOptions, IDataContextStack context)
        {
            Expression expression = null;
            Exception parsingError = null;
            try
            {
                expression = bindingParser.Parse(node.Value, (DataContextStack)context, bindingOptions);
            }
            catch (Exception exception)
            {
                parsingError = exception;
            }
            return treeBuilder.BuildBinding(bindingOptions, node, expression, context, parsingError);
        }

        protected override object ConvertValue(string value, ITypeDescriptor propertyType)
        {
            return ReflectionUtils.ConvertValue(value, ((ResolvedTypeDescriptor)propertyType).Type);
        }

        protected override ITypeDescriptor GetDataContextChange(IDataContextStack dataContext, IAbstractControl control, IPropertyDescriptor property)
        {
            var type = DataContextChangeAttribute.GetDataContextExpression((DataContextStack)dataContext, (ResolvedControl)control, (DotvvmProperty)property);
            return type != null ? new ResolvedTypeDescriptor(type) : null;
        }

        protected override ITypeDescriptor FindType(string fullTypeNameWithAssembly)
        {
            var type = ReflectionUtils.FindType(fullTypeNameWithAssembly);
            if (type == null) return null;
            return new ResolvedTypeDescriptor(type);
        }

        protected override IPropertyDescriptor FindGlobalProperty(string name)
        {
            return DotvvmProperty.ResolveProperty(name, caseSensitive: false);
        }

    }
}