using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using DotVVM.AutoUI.Annotations;
using DotVVM.AutoUI.Configuration;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.AutoUI
{
    public class AutoUIContext
    {
        public DataContextStack DataContextStack { get; }
        public IServiceProvider Services { get; }

        public IViewModelValidationMetadataProvider ValidationMetadataProvider { get; }

        public IPropertyDisplayMetadataProvider PropertyDisplayMetadataProvider { get; }
        public AutoUIConfiguration AutoUiConfiguration { get; }

        public Type EntityType => DataContextStack.DataContextType;

        public string? ViewName { get; set; }

        public string? GroupName { get; set; }

        public BindingCompilationService BindingService { get; }

        public AutoUIContext(DataContextStack dataContextStack, IServiceProvider services)
        {
            DataContextStack = dataContextStack;
            Services = services;

            ValidationMetadataProvider = services.GetRequiredService<IViewModelValidationMetadataProvider>();
            PropertyDisplayMetadataProvider = services.GetRequiredService<IPropertyDisplayMetadataProvider>();
            AutoUiConfiguration = services.GetRequiredService<AutoUIConfiguration>();
            BindingService = services.GetRequiredService<BindingCompilationService>();
        }

        public IValueBinding CreateValueBinding(PropertyDisplayMetadata property)
        {
            if (property.ValueBinding is not null)
                return property.ValueBinding;

            if (property.PropertyInfo is null)
                throw new ArgumentException("property.PropertyInfo is null => cannot create value binding for this property");

            var s = this.BindingService;
            return s.Cache.CreateCachedBinding("AutoUI-Value", new object?[] { property.PropertyInfo, DataContextStack }, () => {
                var _this = Expression.Parameter(DataContextStack.DataContextType, "_this").AddParameterAnnotation(new BindingParameterAnnotation(DataContextStack));
                var expr = Expression.Property(_this, property.PropertyInfo);
                return (IValueBinding)BindingService.CreateBinding(typeof(ValueBindingExpression<>), new object[] {
                    DataContextStack,
                    new ParsedExpressionBindingProperty(expr)
                });
            });
        }

        public DataContextStack CreateChildDataContextStack(DataContextStack dataContextStack, params Type[] nestedDataContextTypes)
        {
            foreach (var type in nestedDataContextTypes)
            {
                dataContextStack = DataContextStack.Create(type, dataContextStack, dataContextStack.NamespaceImports, dataContextStack.ExtensionParameters, dataContextStack.BindingPropertyResolvers);
            }
            return dataContextStack;
        }

        public DataContextStack CreateChildDataContextStack(params Type[] nestedDataContextTypes) =>
            CreateChildDataContextStack(DataContextStack, nestedDataContextTypes);

        public IViewContext CreateViewContext()
        {
            return new ViewContext()
            {
                ViewName = ViewName,
                GroupName = GroupName
            };
        }
    }
}
