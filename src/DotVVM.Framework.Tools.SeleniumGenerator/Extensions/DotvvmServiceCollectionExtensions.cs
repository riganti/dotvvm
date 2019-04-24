using System;
using DotVVM.Framework.Tools.SeleniumGenerator.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Extensions
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