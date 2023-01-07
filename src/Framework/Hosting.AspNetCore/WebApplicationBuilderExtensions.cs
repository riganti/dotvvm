#if NET6_0
#nullable enable
using System;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddDotVVM(this WebApplicationBuilder builder, Action<IDotvvmServiceCollection>? setupServices = null, Action<DotvvmConfiguration>? setupConfiguration = null)
    {
        builder.Services.AddDataProtection();
        builder.Services.AddAuthorization();
        builder.Services.AddWebEncoders();
        builder.Services.AddAuthentication();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddDotVVM(new DelegateDotvvmServiceConfigurator(setupServices));
        builder.Services.AddSingleton<IDotvvmStartup>(new DelegateDotvvmStartup(setupConfiguration));
        return builder;
    }
}

internal class DelegateDotvvmServiceConfigurator : IDotvvmServiceConfigurator
{
    private readonly Action<IDotvvmServiceCollection>? setupServices;

    public DelegateDotvvmServiceConfigurator(Action<IDotvvmServiceCollection>? setupServices)
    {
        this.setupServices = setupServices;
    }

    public void ConfigureServices(IDotvvmServiceCollection options)
    {
        options.AddDefaultTempStorages("temp");

        setupServices?.Invoke(options);
    }
}

internal class DelegateDotvvmStartup : IDotvvmStartup
{
    private readonly Action<DotvvmConfiguration>? setupConfiguration;

    public DelegateDotvvmStartup(Action<DotvvmConfiguration>? setupConfiguration)
    {
        this.setupConfiguration = setupConfiguration;
    }

    public void Configure(DotvvmConfiguration config, string applicationPath)
    {
        setupConfiguration?.Invoke(config);
    }
}
#endif
