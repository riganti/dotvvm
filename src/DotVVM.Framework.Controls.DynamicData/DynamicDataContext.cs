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
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Binding.Properties;

namespace DotVVM.Framework.Controls.DynamicData
{
    public class DynamicDataContext
    {
        public DataContextStack DataContextStack { get; }

        public IDotvvmRequestContext RequestContext { get; }

        public IViewModelValidationMetadataProvider ValidationMetadataProvider { get; }

        public IPropertyDisplayMetadataProvider PropertyDisplayMetadataProvider { get; }
        public DynamicDataConfiguration DynamicDataConfiguration { get; }

        public Type EntityType => DataContextStack.DataContextType;


        public string ViewName { get; set; }

        public string GroupName { get; set; }



        public Dictionary<StateBagKey, object> StateBag { get; } = new Dictionary<StateBagKey, object>();
        public BindingCompilationService BindingCompilationService { get; }

        public DynamicDataContext(DataContextStack dataContextStack, IDotvvmRequestContext requestContext)
        {
            DataContextStack = dataContextStack;
            RequestContext = requestContext;

            ValidationMetadataProvider = requestContext.Configuration.ServiceProvider.GetRequiredService<IViewModelValidationMetadataProvider>();
            PropertyDisplayMetadataProvider = requestContext.Configuration.ServiceProvider.GetRequiredService<IPropertyDisplayMetadataProvider>();
            DynamicDataConfiguration = requestContext.Configuration.ServiceProvider.GetRequiredService<DynamicDataConfiguration>();
            BindingCompilationService = requestContext.Configuration.ServiceProvider.GetRequiredService<BindingCompilationService>();
        }


        public ValueBindingExpression CreateValueBinding(string expression, params Type[] nestedDataContextTypes)
        {
            return CompileValueBindingExpression(expression, nestedDataContextTypes);
        }


        private ValueBindingExpression CompileValueBindingExpression(string expression, params Type[] nestedDataContextTypes)
        {
            var dataContextStack = CreateDataContextStack(DataContextStack, nestedDataContextTypes);

            var bindingOptions = BindingParserOptions.Create<ValueBindingExpression>();


            var properties = new object[]
            {
                new BindingParserOptions(typeof(ValueBindingExpression)),
                dataContextStack,
                new OriginalStringBindingProperty(expression)
            };

            return new ValueBindingExpression(BindingCompilationService, properties);
        }

        public CommandBindingExpression CreateCommandBinding(string expression, params Type[] nestedDataContextTypes)
        {
            return CompileCommandBindingExpression(expression, nestedDataContextTypes);
        }

        private CommandBindingExpression CompileCommandBindingExpression(string expression, params Type[] nestedDataContextTypes)
        {
            var dataContextStack = CreateDataContextStack(DataContextStack, nestedDataContextTypes);

            var bindingOptions = BindingParserOptions.Create<CommandBindingExpression>();

            var bindingId = Convert.ToBase64String(Encoding.ASCII.GetBytes(dataContextStack.DataContextType.Name + "." + expression));

            var properties = new object[]{
                dataContextStack,
                new OriginalStringBindingProperty(expression),
                new IdBindingProperty(bindingId)
            };

            return new CommandBindingExpression(BindingCompilationService, properties);
        }

        private DataContextStack CreateDataContextStack(DataContextStack dataContextStack, Type[] nestedDataContextTypes)
        {
            foreach (var type in nestedDataContextTypes)
            {
                dataContextStack = DataContextStack.Create(type, dataContextStack, dataContextStack.NamespaceImports, dataContextStack.ExtensionParameters);
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