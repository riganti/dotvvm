#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.ViewModel.Validation
{
    internal class ModelStateDecoratorContext
    {
        internal readonly IDictionary<object, List<ViewModelValidationError>> ValidationErrorsLookup;
        internal readonly object? ValidationTarget;
        internal readonly ISet<object> AlreadyProcessedNodes;

        internal string? ValidationTargetPath { get; set; }

        public ModelStateDecoratorContext(object? validationTarget, IEnumerable<ViewModelValidationError> errors)
        {
            this.ValidationErrorsLookup = errors.GroupBy(e => e.TargetObject).ToDictionary(e => e.Key, e => e.ToList());
            this.ValidationTarget = validationTarget;
            this.AlreadyProcessedNodes = new HashSet<object>();
        }
    }

    internal class ModelStateDecorator : IModelStateDecorator
    {
        private readonly IViewModelSerializationMapper viewModelSerializationMapper;

        public ModelStateDecorator(IViewModelSerializationMapper viewModelMapper)
        {
            this.viewModelSerializationMapper = viewModelMapper;
        }

        public void Decorate(ModelState modelState, object viewModel, List<ViewModelValidationError> errors)
        {
            // Add information about absolute paths to errors
            var modelStateDecoratorContext = new ModelStateDecoratorContext(modelState.ValidationTarget, errors);
            Decorate(viewModel, "", modelStateDecoratorContext);

            // Fix validation target path
            modelState.ValidationTargetPath = modelStateDecoratorContext.ValidationTargetPath!;
            modelState.Errors.AddRange(errors);

            // Remove not found errors
            errors.RemoveAll(error => !modelStateDecoratorContext.AlreadyProcessedNodes.Contains(error.TargetObject));
        }

        private void Decorate(object? viewModel, string pathPrefix, ModelStateDecoratorContext context)
        {
            if (viewModel == null || context.AlreadyProcessedNodes.Contains(viewModel))
                return;

            if (viewModel == context.ValidationTarget)
                context.ValidationTargetPath = (pathPrefix != string.Empty) ? pathPrefix : "/";

            context.AlreadyProcessedNodes.Add(viewModel);
            var viewModelType = viewModel.GetType();
         
            if (ReflectionUtils.IsEnumerable(viewModelType))
            {
                // Traverse each element of a collection
                var index = 0;
                foreach (var item in (IEnumerable)viewModel)
                    Decorate(item, $"{pathPrefix}/{index++}", context);

                return;
            }
            else
            {
                // Traverse all serializable properties
                var map = viewModelSerializationMapper.GetMap(viewModel.GetType());
                foreach (var property in map.Properties.Where(p => p.TransferToServer))
                {
                    var value = property.PropertyInfo.GetValue(viewModel);
                    if (value == null)
                        continue;

                    if (ReflectionUtils.IsComplexType(property.Type))
                        Decorate(value, $"{pathPrefix}/{property.Name}", context);
                }
            }

            // Check if have assigned validation errors to this object
            if (context.ValidationErrorsLookup.TryGetValue(viewModel, out var validationErrors))
            {
                foreach (var validationError in validationErrors)
                {
                    var propertyName = validationError.PropertyPath;
                    var absolutePath = $"{pathPrefix}/{propertyName}".TrimEnd('/');
                    validationError.PropertyPath = absolutePath;
                }
            }
        }
    }
}
