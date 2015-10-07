using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Storage;
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
            config.RouteTable.Add("Sample2", "Sample2", "sample2.dothtml");
            config.RouteTable.Add("Sample3", "Sample3", "sample3.dothtml");
            config.RouteTable.Add("Sample4", "Sample4", "sample4.dothtml");
            config.RouteTable.Add("Sample5", "Sample5", "sample5.dothtml");
            config.RouteTable.Add("Sample6", "Sample6", "sample6.dothtml");
            config.RouteTable.Add("Sample8", "Sample8", "sample8.dothtml");
            config.RouteTable.Add("Sample9", "Sample9", "sample9.dothtml");
            config.RouteTable.Add("Sample10", "Sample10", "sample10.dothtml");
            config.RouteTable.Add("Sample11", "Sample11", "sample11.dothtml");
            config.RouteTable.Add("Sample12", "Sample12", "sample12.dothtml");
            config.RouteTable.Add("Sample13", "Sample13", "sample13.dothtml");
            config.RouteTable.Add("Sample14", "Sample14", "sample14.dothtml");
            config.RouteTable.Add("Sample15", "Sample15", "sample15.dothtml");
            config.RouteTable.Add("Sample16", "Sample16", "sample16.dothtml");
            config.RouteTable.Add("Sample17_SPA", "Sample17", "sample17.dothtml");
            config.RouteTable.Add("Sample17_A", "Sample17/A/{Id}", "sample17_a.dothtml");
            config.RouteTable.Add("Sample17_B", "Sample17/B", "sample17_b.dothtml");
            config.RouteTable.Add("Sample18", "Sample18", "sample18.dothtml");
            config.RouteTable.Add("Sample19", "Sample19", "sample19.dothtml");
            config.RouteTable.Add("Sample20", "Sample20", "sample20.dothtml");
            config.RouteTable.Add("Sample22", "Sample22", "sample22.dothtml");
            config.RouteTable.Add("Sample22-PageDetail", "Sample22/{Id}", "sample22.dothtml");
            config.RouteTable.Add("Sample23", "Sample23", "sample23.dothtml");
            config.RouteTable.Add("Sample24", "Sample24", "sample24.dothtml");
            config.RouteTable.Add("Sample25", "Sample25", "sample25.dothtml");
            config.RouteTable.Add("Sample26", "Sample26", "sample26.dothtml");
            config.RouteTable.Add("Sample27", "Sample27", "sample27.dothtml");
            config.RouteTable.Add("Sample28", "Sample28", "sample28.dothtml");
            config.RouteTable.Add("Sample29", "Sample29", "sample29.dothtml");
            config.RouteTable.Add("Sample30", "Sample30", "sample30.dothtml");
            config.RouteTable.Add("Sample31", "Sample31", "sample31.dothtml");
            config.RouteTable.Add("Sample32", "Sample32", "sample32.dothtml");
            config.RouteTable.Add("Sample33", "Sample33", "sample33.dothtml");
            config.RouteTable.Add("Sample34", "Sample34", "sample34.dothtml");
            config.RouteTable.Add("Sample35", "Sample35", "sample35.dothtml");
            config.RouteTable.Add("Sample36", "Sample36", "sample36.dothtml");
            config.RouteTable.Add("Sample37", "Sample37", "sample37.dothtml");
            config.RouteTable.Add("Sample38", "Sample38", "sample38.dothtml");
            config.RouteTable.Add("AuthSampleLogin", "AuthSample/Login", "AuthSample/login.dothtml");
            config.RouteTable.Add("AuthSamplePage", "AuthSample/SecuredPage", "AuthSample/securedPage.dothtml");
            config.RouteTable.Add("ReturnFileSample", "ReturnFileSample", "ReturnFileSample/sample.dothtml");

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