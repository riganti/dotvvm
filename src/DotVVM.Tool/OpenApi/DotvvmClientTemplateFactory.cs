using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NJsonSchema.CodeGeneration;
using DefaultTemplateFactory = NSwag.CodeGeneration.DefaultTemplateFactory;

namespace DotVVM.Tool.OpenApi
{
    public class DotvvmClientTemplateFactory : DefaultTemplateFactory
    {
        public DotvvmClientTemplateFactory(
            CodeGeneratorSettingsBase settings,
            Assembly[] assemblies)
            : base(settings, assemblies)
        {
        }

        protected override string GetEmbeddedLiquidTemplate(string language, string template)
        {
            var resourceName = "DotVVM.Tool.OpenApi." + template + ".liquid";

            var resource = typeof(ApiClientManager).Assembly.GetManifestResourceStream(resourceName);
            if (resource != null)
            {
                using var reader = new StreamReader(resource);
                return reader.ReadToEnd();
            }

            return base.GetEmbeddedLiquidTemplate(language, template);
        }
    }
}
