using System.ComponentModel.DataAnnotations;

namespace DotVVM.Framework.ViewModel.Validation.ClientValidation
{

    public abstract class ClientValidationFactoryBase<T> : IClientValidationFactory<T>
    {
        public abstract string Name { get; }

        public virtual object[] GetParameters(T attribute)
        {
            return new object[] { };
        }
    }

    public abstract class ClientValidationFactoryBase : IClientValidationFactory
    {
        public abstract string Name { get; }
        public abstract object[] GetParameters(ValidationAttribute attribute);

        public IViewModelPropertyValidationRule CreateViewModelPropertyValidationRule(ValidationAttribute attribute, string propertyName)
        {
            return new ClientValidationFactoryToIViewModelPropertyValidationRuleAdapter(this, attribute, propertyName);
        }

        private class ClientValidationFactoryToIViewModelPropertyValidationRuleAdapter : IViewModelPropertyValidationRule
        {
            public ClientValidationFactoryToIViewModelPropertyValidationRuleAdapter(IClientValidationFactory clientValidationFactory, ValidationAttribute validationAttribute, string propertyName)
            {
                ClientRuleName = clientValidationFactory.Name;
                SourceValidationAttribute = validationAttribute;
                Parameters = clientValidationFactory.GetParameters(validationAttribute);
                ErrorMessage = validationAttribute.FormatErrorMessage(propertyName);
            }

            public string ClientRuleName { get; }
            public string ErrorMessage { get; }
            public object[] Parameters { get; }
            public ValidationAttribute SourceValidationAttribute { get; }
        }
    }
}