#if NET6_0
#nullable enable
using System;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds DotVVM services with all its dependencies including authorization and data protection to the specified <see cref="WebApplicationBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder" /> to add services to.</param>
    // ReSharper disable once InconsistentNaming
    public static WebApplicationBuilder AddDotVVM<TServiceConfigurator>(this WebApplicationBuilder builder)
        where TServiceConfigurator : IDotvvmServiceConfigurator, new()
    {
        AddDotvvmServiceDependencies(builder);
        builder.Services.AddDotVVM<TServiceConfigurator>();
        return builder;
    }

    /// <summary>
    /// Adds DotVVM services with all its dependencies including authorization and data protection to the specified <see cref="WebApplicationBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder" /> to add services to.</param>
    /// <param name="configurator">The <see cref="IDotvvmServiceConfigurator"/> instance.</param>
    public static WebApplicationBuilder AddDotVVM(this WebApplicationBuilder builder, IDotvvmServiceConfigurator configurator)
    {
        AddDotvvmServiceDependencies(builder);
        builder.Services.AddDotVVM(configurator);
        return builder;
    }

    /// <summary>
    /// Adds DotVVM services with all its dependencies including authorization and data protection to the specified <see cref="WebApplicationBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder" /> to add services to.</param>
    // ReSharper disable once InconsistentNaming
    public static WebApplicationBuilder AddDotVVM(this WebApplicationBuilder builder)
    {
        AddDotvvmServiceDependencies(builder);
        builder.Services.AddDotVVM();
        return builder;
    }

    private static void AddDotvvmServiceDependencies(WebApplicationBuilder builder)
    {
        builder.Services.AddDataProtection();
        builder.Services.AddAuthorization();
        builder.Services.AddWebEncoders();
        builder.Services.AddAuthentication();
    }
}
#endif
