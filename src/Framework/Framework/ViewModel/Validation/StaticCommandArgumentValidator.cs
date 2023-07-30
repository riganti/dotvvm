using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ViewModel.Validation
{
    /// <summary> Default implementation of <see cref="IStaticCommandArgumentValidator" />. Validates arguments using <see cref="IViewModelValidator" /> plus validates any <see cref="ValidationAttribute"/> placed on the method parameters. </summary>
    public class StaticCommandArgumentValidator: IStaticCommandArgumentValidator
    {
        public StaticCommandArgumentValidator(IViewModelValidator viewModelValidator, IValidationErrorPathExpander validationErrorPathExpander)
        {
            this.viewModelValidator = viewModelValidator;
            this.validationErrorPathExpander = validationErrorPathExpander;
        }

        readonly IViewModelValidator viewModelValidator;
        readonly IValidationErrorPathExpander validationErrorPathExpander;

        public StaticCommandModelState? ValidateStaticCommand(StaticCommandInvocationPlan staticCommandInvocation, object?[] args, IDotvvmRequestContext context)
        {
            var invokedMethod = staticCommandInvocation.Method;
            // only enable validation on arguments sent from the client
            var allowValidation = staticCommandInvocation.Arguments.Select(a => a.Type == StaticCommandParameterType.Argument).ToArray();
            ParameterInfo?[] parameters = invokedMethod.GetParameters();
            if (!invokedMethod.IsStatic)
                parameters = Enumerable.Concat(new ParameterInfo?[] { null }, parameters).ToArray();
            if (parameters.Length != args.Length) throw new ArgumentException("parameters.Length != args.Length");

            var modelState = new StaticCommandModelState();

            // validate DataAnnotations.ValidationAttribute on parameters
            for (int i = 0; i < args.Length; i++)
            {
                if (!allowValidation[i])
                    continue;
                var argument = args[i];
                var parameter = parameters[i];
                // validation attributes on parameter
                if (parameter is not null && parameter.IsDefined(typeof(ValidationAttribute)))
                {
                    foreach (var validator in parameter.GetCustomAttributes<ValidationAttribute>())
                    {
                        if (!validator.IsValid(args[i]))
                        {
                            var name = parameter.Name.NotNull();
                            modelState.AddArgumentError(name, validator.FormatErrorMessage(name));
                        }
                    }
                }
                // validate data annotations in the object
                if (argument is {} && !ReflectionUtils.IsDotvvmNativePrimitiveType(argument.GetType()))
                {
                    var errors = this.viewModelValidator.ValidateViewModel(args[i]);
                    foreach (var e in errors)
                    {
                        if (!e.IsResolved && e.TargetObject != argument)
                            throw new Exception($"Validation error {e} is not resolved and its target object is not the same as the argument.");
                        if (e.PropertyPath is null)
                            throw new Exception($"Validation error {e} has no property path.");

                        modelState.AddRawArgumentError(parameter is null ? "this" : parameter.Name.NotNull(), e.PropertyPath, e.ErrorMessage);
                    }
                }

            }

            if (modelState.IsValid)
                return null;
            else
                return modelState;
        }
    }
}
