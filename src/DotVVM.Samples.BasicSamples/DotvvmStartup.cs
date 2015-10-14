using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Storage;
using DotVVM.Framework.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DotVVM.Samples.BasicSamples
{
    public class DotvvmStartup : IDotvvmStartup
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Sample1", "Sample1/{id?:int}", "sample1.dothtml");
            config.RouteTable.Add("Sample17_SPA", "Sample17", "sample17.dothtml");
            config.RouteTable.Add("Sample17_A", "Sample17/A/{Id}", "sample17_a.dothtml");
            config.RouteTable.Add("Sample17_B", "Sample17/B", "sample17_b.dothtml");
            config.RouteTable.Add("Sample22-PageDetail", "Sample22/{Id}", "sample22.dothtml");

            config.AutoRegisterRoutes();

            var bundles = new BundlingResourceProcessor();
            bundles.RegisterBundle(config.Resources.FindNamedResource("testJsBundle"), "testJs", "testJs2");
            config.Resources.DefaultResourceProcessors.Add(bundles);

            //config.Styles.Register<Repeater>()
            //    .SetAttribute("class", "repeater")
            //    .SetProperty(r => r.WrapperTagName, "div");

            config.ServiceLocator.RegisterSingleton<IUploadedFileStorage>(
                () => new FileSystemUploadedFileStorage(Path.Combine(applicationPath, "TempUpload"), TimeSpan.FromMinutes(30)));

            config.ServiceLocator.RegisterSingleton<IReturnedFileStorage>(() =>
                new FileSystemReturnedFileStorage(Path.Combine(applicationPath, "TempFolder"), TimeSpan.FromMinutes(1)));
        }
    }
}