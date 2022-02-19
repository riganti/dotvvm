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
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DotVVM.Framework.Controls.DynamicData
{
    public class DynamicDataContext
    {
        private static ConcurrentDictionary<BindingDescriptor, IBinding> bindingCache = new ConcurrentDictionary<BindingDescriptor, IBinding>();

        public DataContextStack DataContextStack { get; }
        public IServiceProvider Services { get; }

        public IViewModelValidationMetadataProvider ValidationMetadataProvider { get; }

        public IPropertyDisplayMetadataProvider PropertyDisplayMetadataProvider { get; }
        public DynamicDataConfiguration DynamicDataConfiguration { get; }

        public Type EntityType => DataContextStack.DataContextType;

        public string? ViewName { get; set; }

        public string? GroupName { get; set; }

        public Dictionary<StateBagKey, object> StateBag { get; } = new Dictionary<StateBagKey, object>();
        public BindingCompilationService BindingService { get; }

        public DynamicDataContext(DataContextStack dataContextStack, IServiceProvider services)
        {
            DataContextStack = dataContextStack;
            Services = services;

            ValidationMetadataProvider = services.GetRequiredService<IViewModelValidationMetadataProvider>();
            PropertyDisplayMetadataProvider = services.GetRequiredService<IPropertyDisplayMetadataProvider>();
            DynamicDataConfiguration = services.GetRequiredService<DynamicDataConfiguration>();
            BindingService = services.GetRequiredService<BindingCompilationService>();
        }

        public IValueBinding CreateValueBinding(PropertyDisplayMetadata property)
        {
            var s = this.BindingService;
            return s.Cache.CreateCachedBinding("DD-Value", new object[] { property.PropertyInfo, DataContextStack }, () => {
                var _this = Expression.Parameter(DataContextStack.DataContextType, "_this").AddParameterAnnotation(new BindingParameterAnnotation(DataContextStack));
                var expr = Expression.Property(_this, property.PropertyInfo);
                return (IValueBinding)BindingService.CreateBinding(typeof(ValueBindingExpression<>), new object[] {
                    DataContextStack,
                    new ParsedExpressionBindingProperty(expr)
                });
            });
        }

        public IValueBinding CreateValueBinding(string expression, params Type[] nestedDataContextTypes)
        {
            var dataContextStack = CreateDataContextStack(DataContextStack, nestedDataContextTypes);

            var descriptor = new BindingDescriptor(expression, typeof(ValueBindingExpression), dataContextStack);

            return (ValueBindingExpression)bindingCache.GetOrAdd(descriptor, bd => CompileValueBindingExpression(descriptor));
        }

        public IValueBinding<T> CreateValueBinding<T>(string expression, params Type[] nestedDataContextTypes)
        {
            var dataContextStack = CreateDataContextStack(DataContextStack, nestedDataContextTypes);

            var descriptor = new BindingDescriptor(expression, typeof(ValueBindingExpression<T>), dataContextStack);

            return (ValueBindingExpression<T>)bindingCache.GetOrAdd(descriptor, bd => CompileValueBindingExpression(descriptor));
        }

        public CommandBindingExpression CreateCommandBinding(string expression, params Type[] nestedDataContextTypes)
        {
            var dataContextStack = CreateDataContextStack(DataContextStack, nestedDataContextTypes);

            var descriptor = new BindingDescriptor(expression, typeof(CommandBindingExpression), dataContextStack);

            return (CommandBindingExpression)bindingCache.GetOrAdd(descriptor, bd => CompileCommandBindingExpression(descriptor));
        }

        private IBinding CompileCommandBindingExpression(BindingDescriptor descriptor)
        {
            var bindingId = Convert.ToBase64String(Encoding.ASCII.GetBytes(descriptor.DataContextStack.DataContextType.Name + "." + descriptor.Expression));

            var properties = new object[]{
                descriptor.DataContextStack,
                new OriginalStringBindingProperty(descriptor.Expression),
                new IdBindingProperty(bindingId)
            };

            return BindingService.CreateBinding(descriptor.BindingType, properties);
        }

        private IBinding CompileValueBindingExpression(BindingDescriptor bindingDescriptor)
        {
            var properties = new object[]
            {
                bindingDescriptor.DataContextStack,
                new OriginalStringBindingProperty(bindingDescriptor.Expression)
            };

            return BindingService.CreateBinding(bindingDescriptor.BindingType, properties);
        }

        private DataContextStack CreateDataContextStack(DataContextStack dataContextStack, Type[] nestedDataContextTypes)
        {
            foreach (var type in nestedDataContextTypes)
            {
                dataContextStack = DataContextStack.Create(type, dataContextStack, dataContextStack.NamespaceImports, dataContextStack.ExtensionParameters);
            }
            return dataContextStack;
        }

        public IViewContext CreateViewContext()
        {
            return new ViewContext()
            {
                ViewName = ViewName,
                GroupName = GroupName
            };
        }

        private class BindingDescriptor
        {
            public DataContextStack DataContextStack { get; }
            public string Expression { get; }

            public Type BindingType { get; }

            public BindingDescriptor(string expression, Type bindingType, DataContextStack dataContextStack)
            {
                Expression = expression ?? throw new ArgumentNullException(nameof(expression));
                BindingType = bindingType ?? throw new ArgumentNullException(nameof(bindingType));
                DataContextStack = dataContextStack ?? throw new ArgumentNullException(nameof(dataContextStack)); ;
            }

            public override bool Equals(object? obj)
            {
                var descriptor = obj as BindingDescriptor;

                if (descriptor == null)
                {
                    return false;
                }

                return DataContextStack.Equals(descriptor.DataContextStack)
                    && Expression.Equals(descriptor.Expression)
                    && BindingType.Equals(descriptor.BindingType);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = 0;
                    hashCode = (hashCode * 397) ^ (DataContextStack?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 13) ^ (Expression?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 49) ^ (BindingType?.GetHashCode() ?? 0);

                    return hashCode;
                }
            }
        }
    }
}
