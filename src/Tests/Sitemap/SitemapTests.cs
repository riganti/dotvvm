using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Testing;
using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Providers;
using DotVVM.Sitemap.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Sitemap;

[TestClass]
public class SitemapTests
{
    OutputChecker check = new OutputChecker("testoutputs");

    [TestMethod]
    public async Task SingleRoute()
    {
        var config = CreateConfig();
        config.RouteTable.Add("SingleRoute", "single-route", "single-route.dothtml")
            .WithSitemapOptions();

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task SingleRouteWithPriorityAndLastModDateAndChangeFrequency()
    {
        var config = CreateConfig();
        config.RouteTable.Add("SingleRoute", "single-route", "single-route.dothtml")
            .WithSitemapOptions(s =>
            {
                s.Priority = 0.5;
                s.LastModified = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                s.ChangeFrequency = ChangeFrequency.Weekly;
            });

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task SingleRouteOptionsInheritance()
    {
        var config = CreateConfig();
        config.RouteTable
            .WithDefaultSitemapOptions(s => s.Priority = 0.5)
            .AddGroup("Group", "group", "group", g => {
                g
                    .Add("SingleRoute", "single-route", "single-route.dothtml")
                    .WithSitemapOptions(s => {
                        s.LastModified = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    });
            })
            .WithDefaultSitemapOptions(s => s.ChangeFrequency = ChangeFrequency.Always);

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task SingleRouteOptionsOverriding()
    {
        var config = CreateConfig();
        config.RouteTable
            .WithDefaultSitemapOptions(s => s.Priority = 0.1)
            .AddGroup("Group", "group", "group", g => {
                g
                    .Add("SingleRoute", "single-route", "single-route.dothtml")
                    .WithSitemapOptions(s => {
                        s.Priority = 0.3;
                    });
            })
            .WithDefaultSitemapOptions(s => s.Priority = 0.2);

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task MultipleRoutesExcluding()
    {
        var config = CreateConfig();
        config.RouteTable
            .AddGroup("Group", "group", "group", g => {
                g.Add("SingleRoute", "single-route", "single-route.dothtml");
                g.Add("ExcludedRoute", "excluded-route", "excluded-route.dothtml")
                    .WithSitemapOptions(s => s.Exclude = true);
            })
            .WithDefaultSitemapOptions(s => s.Priority = 1);

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task MultipleRoutesExcludingGroup()
    {
        var config = CreateConfig();
        config.RouteTable
            .AddGroup("Group", "group", "group", g => {
                g.Add("SingleRoute", "single-route", "single-route.dothtml")
                    .WithSitemapOptions(s => s.Exclude = false);
                g.Add("ExcludedRoute", "excluded-route", "excluded-route.dothtml");
            })
            .WithDefaultSitemapOptions(s => s.Exclude = true);

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task RouteWithParameters_NoProvider()
    {
        var config = CreateConfig();
        config.RouteTable.Add("SingleRoute", "single-route/{id}", "single-route.dothtml")
            .WithSitemapOptions();

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task RouteWithParameters_WithProvider()
    {
        var config = CreateConfig(s => {
            s.AddSingleton<ISitemapEntryProvider>(new DelegateSitemapEntryProvider(routes => {
                var r = routes.Single(r => r.Route.RouteName == "SingleRoute");
                r.AddSitemapEntry(new { Id = "1" });
                r.AddSitemapEntry(new { Id = "2" });
            }));
        });
        config.RouteTable.Add("SingleRoute", "single-route/{id}", "single-route.dothtml")
            .WithSitemapOptions();

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task RouteWithParameters_WithProvider_MissingParams()
    {
        var config = CreateConfig(s => {
            s.AddSingleton<ISitemapEntryProvider>(new DelegateSitemapEntryProvider(routes => {
                var r = routes.Single(r => r.Route.RouteName == "SingleRoute");
                r.AddSitemapEntry(new Dictionary<string, object>() { { "Id", "1" } });
            }));
        });
        config.RouteTable.Add("SingleRoute", "single-route/{id}/{Id2}", "single-route.dothtml")
            .WithSitemapOptions();

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await GenerateXml(config));
        Assert.IsTrue(ex.Message.Contains("Ensure that the supplied parameter names correspond to the route parameters (id, Id2)."));
    }

    [TestMethod]
    public async Task RouteWithParameters_WithProvider_OverridesOptions()
    {
        var config = CreateConfig(s => {
            s.AddSingleton<ISitemapEntryProvider>(new DelegateSitemapEntryProvider(routes => {
                var r = routes.Single(r => r.Route.RouteName == "SingleRoute");
                r.AddSitemapEntry(new { Id = "1" }, overrideOptions: new RouteSitemapOptions() { Priority = 0.5 });
                r.AddSitemapEntry(new { Id = "2" });
            }));
        });
        config.RouteTable.Add("SingleRoute", "single-route/{id}", "single-route.dothtml")
            .WithSitemapOptions();

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task SingleLocalizedRoute()
    {
        var config = CreateConfig();
        config.RouteTable.Add("SingleRoute", "single-route", "single-route.dothtml",
                localizedUrls: [
                    new LocalizedRouteUrl("cs", "jedna-routa")
                ])
            .WithSitemapOptions();

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task SingleLocalizedRouteWithParameters_NoProvider()
    {
        var config = CreateConfig();
        config.RouteTable.Add("SingleRoute", "single-route/{id}", "single-route.dothtml",
                localizedUrls: [
                    new LocalizedRouteUrl("cs", "jedna-routa/{id}")
                ])
            .WithSitemapOptions();

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task SingleLocalizedRouteWithParameters_WithProvider_AllCulturesSameParams()
    {
        var config = CreateConfig(s => {
            s.AddSingleton<ISitemapEntryProvider>(new DelegateSitemapEntryProvider(routes => {
                var r = routes.Single(r => r.Route.RouteName == "SingleRoute");
                r.AddSitemapEntry(new { Id = "1" });
                r.AddSitemapEntry(new { Id = "2" });
            }));
        });
        config.RouteTable.Add("SingleRoute", "single-route/{id}", "single-route.dothtml",
                localizedUrls: [
                    new LocalizedRouteUrl("cs", "jedna-routa/{id}")
                ])
            .WithSitemapOptions();

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task SingleLocalizedRouteWithParameters_WithProvider_CultureSpecificParams()
    {
        var config = CreateConfig(s => {
            s.AddSingleton<ISitemapEntryProvider>(new DelegateSitemapEntryProvider(routes => {
                var r = routes.Single(r => r.Route.RouteName == "SingleRoute");
                r.AddSitemapEntry(new { Id = "one" }, new Dictionary<string, object>() { { "cs", new { Id = "jedna" } } });
                r.AddSitemapEntry(new { Id = "two" }, new Dictionary<string, object>() { { "cs", new { Id = "dva" } } });
            }));
        });
        config.RouteTable.Add("SingleRoute", "single-route/{id}", "single-route.dothtml",
                localizedUrls: [
                    new LocalizedRouteUrl("cs", "jedna-routa/{id}")
                ])
            .WithSitemapOptions();

        check.CheckString(await GenerateXml(config), fileExtension: "xml");
    }

    [TestMethod]
    public async Task SingleLocalizedRouteWithParameters_InvalidCultureParameterValue()
    {
        var config = CreateConfig(s => {
            s.AddSingleton<ISitemapEntryProvider>(new DelegateSitemapEntryProvider(routes => {
                var r = routes.Single(r => r.Route.RouteName == "SingleRoute");
                r.AddSitemapEntry(new { Id = "one" }, new Dictionary<string, object>() { { "x", new { Id = "jedna" } } });
                r.AddSitemapEntry(new { Id = "two" });
            }));
        });
        config.RouteTable.Add("SingleRoute", "single-route/{id}", "single-route.dothtml",
                localizedUrls: [
                    new LocalizedRouteUrl("cs", "jedna-routa/{id}")
                ])
            .WithSitemapOptions();

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await GenerateXml(config));
        Assert.IsTrue(ex.Message.Contains("Localized parameter values contain cultures that are not supported by the route: x."));
    }

    [TestMethod]
    public async Task SingleLocalizedRouteWithParameters_OverridingDefaultCultureParameterValue()
    {
        var config = CreateConfig(s => {
            s.AddSingleton<ISitemapEntryProvider>(new DelegateSitemapEntryProvider(routes => {
                var r = routes.Single(r => r.Route.RouteName == "SingleRoute");
                r.AddSitemapEntry(new { Id = "jedna" }, new Dictionary<string, object>() { { "en", new { Id = "one" } } });
                r.AddSitemapEntry(new { Id = "two" });
            }));
        });
        config.RouteTable.Add("SingleRoute", "single-route/{id}", "single-route.dothtml",
                localizedUrls: [
                    new LocalizedRouteUrl("cs", "jedna-routa/{id}")
                ])
            .WithSitemapOptions();

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await GenerateXml(config));
        Assert.IsTrue(ex.Message.Contains("Localized parameter values contain cultures that are not supported by the route"));
    }

    private DotvvmConfiguration CreateConfig(Action<IServiceCollection>? configureServices = null)
    {
        return DotvvmTestHelper.CreateConfiguration(s => {
            new DotvvmServiceCollection(s).AddSitemap(opt => opt.DefaultCulture = "en");
            configureServices?.Invoke(s);
        });
    }

    private async Task<string> GenerateXml(DotvvmConfiguration config)
    {
        var resolver = config.ServiceProvider.GetRequiredService<SitemapResolver>();
        var entries = await resolver.GetSitemapEntriesAsync(DotvvmTestHelper.CreateContext(configuration: config), new Uri("http://localhost/"), CancellationToken.None);
        var xmlBuilder = config.ServiceProvider.GetRequiredService<SitemapXmlBuilder>();
        var xml = xmlBuilder.BuildXml(entries, new Uri("http://localhost/"));
        return xml.ToString();
    }

    class DelegateSitemapEntryProvider(Action<IReadOnlyList<RouteSitemapContext>> action) : ISitemapEntryProvider
    {
        public Task TryResolveSitemapEntries(IReadOnlyList<RouteSitemapContext> routes, CancellationToken ct)
        {
            action(routes);
            return Task.CompletedTask;
        }
    }
}
