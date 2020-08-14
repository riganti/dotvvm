using System;
using DotVVM.Framework.Testing.Generator;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Configuration
{
    public static class DotvvmServiceCollectionExtensions
    {
        public static void AddSeleniumGenerator(this IDotvvmServiceCollection services,
            Action<SeleniumGeneratorOptions> optionsBuilder = null)
        {
            services.Services.AddSingleton<SeleniumGeneratorOptions>(provider =>
            {
                var options = new SeleniumGeneratorOptions();
                optionsBuilder?.Invoke(options);

                return options;
            });
        }
    }
}
