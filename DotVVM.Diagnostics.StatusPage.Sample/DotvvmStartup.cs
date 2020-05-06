using DotVVM.Diagnostics.StatusPage.Sample.Presenter;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Diagnostics.StatusPage.Sample
{
    public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
    {
        // For more information about this class, visit https://dotvvm.com/docs/tutorials/basics-project-structure
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            ConfigureRoutes(config, applicationPath);
            ConfigureControls(config, applicationPath);
            ConfigureResources(config, applicationPath);
        }

        private void ConfigureRoutes(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Default", "", "Views/default.dothtml");
            config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));
            config.RouteTable.AddGroup("Group1", "group1", "virtualGroup", s =>
            {
                s.Add("group1_rout1", "gr1", b => new NoPresenter());
            });
            config.RouteTable.Add("noPresenter", "presenter/no", b => new NoPresenter());
            config.RouteTable.Add("LocalizablePresenter", "presenter/localizable", "Views/default.dothtml", LocalizablePresenter.BasedOnQuery("lang"));

        }

        private void ConfigureControls(DotvvmConfiguration config, string applicationPath)
        {
            config.Markup.AddMarkupControl("cc", "control", "Controls/control.dotcontrol");
            config.Markup.AddMarkupControl("cc", "control2", "Controls/control2.dotcontrol");
            config.Markup.AddMarkupControl("cc", "ControlError", "Controls/ControlError.dotcontrol");
            config.Markup.AddMarkupControl("cc", "NestedControl", "Controls/NestedControl.dotcontrol");
        }

        private void ConfigureResources(DotvvmConfiguration config, string applicationPath)
        {
            // register custom resources and adjust paths to the built-in resources
            config.Resources.Register("bootstrap-css", new StylesheetResource
            {
                Location = new UrlResourceLocation("~/lib/bootstrap/dist/css/bootstrap.min.css")
            });
            config.Resources.Register("bootstrap-theme", new StylesheetResource
            {
                Location = new UrlResourceLocation("~/lib/bootstrap/dist/css/bootstrap-theme.min.css"),
                Dependencies = new[] { "bootstrap-css" }
            });
            config.Resources.Register("bootstrap", new ScriptResource
            {
                Location = new UrlResourceLocation("~/lib/bootstrap/dist/js/bootstrap.min.js"),
                Dependencies = new[] { "bootstrap-css", "jquery" }
            });
            config.Resources.Register("jquery", new ScriptResource
            {
                Location = new UrlResourceLocation("~/lib/jquery/dist/jquery.min.js")
            });
        }

        public void ConfigureServices(IDotvvmServiceCollection options)
        {
            options.AddStatusPage();
            options.AddDefaultTempStorages("Temp");
        }
    }
}