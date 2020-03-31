# DotVVM Status Page
Compilation Status Page is a site that allows you to easily check what views, controls and masterpages are broken due to compilation errors.

## How it works
DotVVM views are compiled on demand when the page requests a dothtml file. This package adds you one diagnostics page to you dotvvm application. When you access this status page by default on route **_diagnostics/status** all dothtml files registered in DotvvmStartup.cs are requested and compiled.

[sample]: https://raw.githubusercontent.com/riganti/dotvvm-samples-compilation-status-page/42184142d7905be3d2e23661dbb1905c3ed4ba80/docs/sample.PNG ""


## Get Started
 - Install NuGet package [`DotVVM.Diagnostics.StatusPage`](https://www.nuget.org/packages/DotVVM.Diagnostics.StatusPage/)
 - Register status page in your DotvvmStartup 

```
    public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
        }

        public void ConfigureServices(IDotvvmServiceCollection services)
        {
            services.AddStatusPage();
            services.AddDefaultTempStorages("temp");
         }
    }
```


