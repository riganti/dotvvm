using System;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Framework.ViewModel.Validation.ClientValidation
{
    public class ClientValidationFactory : ClientValidationFactoryBase
    {
        private readonly Func<ValidationAttribute, object[]> getParameters;

        public ClientValidationFactory(string name, Func<ValidationAttribute, object[]> getParameters)
        {
            Name = name;
            this.getParameters = getParameters;
        }

        public override string Name { get; }

        public override object[] GetParameters(ValidationAttribute attribute)
        {
            return getParameters(attribute);
        }
    }
}