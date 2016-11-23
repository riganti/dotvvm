using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using DotVVM.Framework.ViewModel.Validation;
using DotVVM.Framework.ViewModel.Validation.ClientValidation;

namespace DotVVM.Framework.Configuration
{    public class ValidationConfiguration
    {
        private static readonly ConcurrentDictionary<Type, IClientValidationFactory> clientValidationAttributes = new ConcurrentDictionary<Type, IClientValidationFactory>();

        public void Register<TAttribute, TAttributeClintValidationFactory>()
            where TAttribute : ValidationAttribute
            where TAttributeClintValidationFactory : IClientValidationFactory<TAttribute>, new()
        {
            Register(new TAttributeClintValidationFactory());
        }

        public void Register<TAttribute>(IClientValidationFactory<TAttribute> clientValidationFactory) where TAttribute : ValidationAttribute
        {
            var value = new ClientValidationFactory(clientValidationFactory.Name, attribute => clientValidationFactory.GetParameters((TAttribute)attribute));
            clientValidationAttributes.AddOrUpdate(typeof(TAttribute), value, (key, oldValue) => value);
        }

        public void Register<TAttribute>(string name, Func<TAttribute, object[]> parametersFactory) where TAttribute : ValidationAttribute
        {
            Register(new SupportedClientValidationFactory<TAttribute>(name, parametersFactory));
        }

        public void RegisterDefault()
        {
            Register<RequiredAttribute, RequiredClientValidationFactory>();
            Register<RegularExpressionAttribute, RegularExpressionClientValidationFactory>();
            Register<RangeAttribute, RangeClientValidationFactory>();
            Register<DotvvmEnforceClientFormatAttribute, DotvvmEnforceClientFormatClientValidationFactory>();
        }

        public IClientValidationFactory GetClientValidationRule<TAttribute>() where TAttribute : ValidationAttribute
        {
            return GetClientValidationRule(typeof(TAttribute));
        }

        public IClientValidationFactory GetClientValidationRule(Type attributeType)
        {
            IClientValidationFactory clientValidationAttribute;
            if (clientValidationAttributes.TryGetValue(attributeType, out clientValidationAttribute))
            {
                return clientValidationAttribute;
            }
            else
            {
                return new NotSupportedClientValidationFactory();
            }
        }

        class SupportedClientValidationFactory<TAttribute> : IClientValidationFactory<TAttribute> where TAttribute : ValidationAttribute
        {
            private readonly Func<TAttribute, object[]> getParameters;

            public SupportedClientValidationFactory(string name, Func<TAttribute, object[]> getParameters)
            {
                this.getParameters = getParameters;
                Name = name;
            }

            public string Name { get; }
            public object[] GetParameters(TAttribute attribute)
            {
                return getParameters(attribute);
            }
        }

        class NotSupportedClientValidationFactory : ClientValidationFactoryBase
        {
            public override string Name => string.Empty;

            public override object[] GetParameters(ValidationAttribute attribute)
            {
                return new object[] {};
            }
        }
    }
}