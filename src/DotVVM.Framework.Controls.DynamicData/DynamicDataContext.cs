using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls.DynamicData.Annotations;
using DotVVM.Framework.Controls.DynamicData.Configuration;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Framework.Controls.DynamicData
{
    public class DynamicDataContext
    {
        public DataContextStack DataContextStack { get; }

        public IDotvvmRequestContext RequestContext { get; }

        public IViewModelValidationMetadataProvider ValidationMetadataProvider { get; }

        public IPropertyDisplayMetadataProvider PropertyDisplayMetadataProvider { get; }

        public Type EntityType => DataContextStack.DataContextType;

        public DynamicDataConfiguration DynamicDataConfiguration { get; }

        public string ViewName { get; set; }

        public string GroupName { get; set; }
                


        public Dictionary<StateBagKey, object> StateBag { get; } = new Dictionary<StateBagKey, object>();


        public DynamicDataContext(DataContextStack dataContextStack, IDotvvmRequestContext requestContext)
        {
            DataContextStack = dataContextStack;
            RequestContext = requestContext;

            ValidationMetadataProvider = requestContext.Configuration.ServiceLocator.GetService<IViewModelValidationMetadataProvider>();
            PropertyDisplayMetadataProvider = requestContext.Configuration.ServiceLocator.GetService<IPropertyDisplayMetadataProvider>();
            DynamicDataConfiguration = requestContext.Configuration.ServiceLocator.GetService<DynamicDataConfiguration>();
        }
        

        public ValueBindingExpression CreateValueBinding(string expression, params Type[] nestedDataContextTypes)
        {
            return CompileValueBindingExpression(expression, nestedDataContextTypes);
        }
        

        private ValueBindingExpression CompileValueBindingExpression(string expression, params Type[] nestedDataContextTypes)
        {
            var dataContextStack = CreateDataContextStack(DataContextStack, nestedDataContextTypes);

            var parser = new BindingExpressionBuilder();
            var bindingOptions = BindingParserOptions.Create<ValueBindingExpression>();
            var parserResult = parser.Parse(expression, dataContextStack, bindingOptions);

            var compiledExpression = new BindingCompilationAttribute().CompileToDelegate(parserResult, dataContextStack, typeof(object));
            var javascript = new BindingCompilationAttribute().CompileToJavascript(new ResolvedBinding()
            {
                Value = expression,
                Expression = parserResult,
                BindingType = typeof(ValueBindingExpression),
                DataContextTypeStack = dataContextStack
            }, null);
            return new ValueBindingExpression(compiledExpression.Compile(), javascript)
            {
                OriginalString = expression
            };
        }

        public CommandBindingExpression CreateCommandBinding(string expression, params Type[] nestedDataContextTypes)
        {
            return CompileCommandBindingExpression(expression, nestedDataContextTypes);
        }

        private CommandBindingExpression CompileCommandBindingExpression(string expression, params Type[] nestedDataContextTypes)
        {
            var dataContextStack = CreateDataContextStack(DataContextStack, nestedDataContextTypes);

            var parser = new BindingExpressionBuilder();
            var bindingOptions = BindingParserOptions.Create<CommandBindingExpression>();
            var parserResult = parser.Parse(expression, dataContextStack, bindingOptions);
            var compiledExpression = new CommandBindingCompilationAttribute().CompileToDelegate(parserResult, dataContextStack, typeof(Command)).Compile();

            var bindingId = Convert.ToBase64String(Encoding.ASCII.GetBytes(dataContextStack.DataContextType.Name + "." + expression));
            return new CommandBindingExpression(compiledExpression, bindingId)
            {
                OriginalString = expression
            };
        }

        private DataContextStack CreateDataContextStack(DataContextStack dataContextStack, Type[] nestedDataContextTypes)
        {
            foreach (var type in nestedDataContextTypes)
            {
                dataContextStack = new DataContextStack(type, dataContextStack, dataContextStack.RootControlType, dataContextStack.NamespaceImports);
            }
            return dataContextStack;
        }

        public IViewContext CreateViewContext(IDotvvmRequestContext context)
        {
            return new ViewContext()
            {
                ViewName = ViewName,
                GroupName = GroupName,
                CurrentUser = context.HttpContext.User
            };
        }
    }
}