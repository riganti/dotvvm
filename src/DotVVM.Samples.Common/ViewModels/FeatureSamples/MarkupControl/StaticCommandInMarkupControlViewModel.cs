using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
    public class DeviceModel
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public List<string> Groups { get; set; }
    }

    public class StaticCommandInMarkupControlViewModel : DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPA.SiteViewModel
    {
        public GridViewDataSet<DeviceModel> Devices { get; set; } = new GridViewDataSet<DeviceModel> {
            PagingOptions = new PagingOptions {
                PageSize = 10
            }
        };

        public DeviceModel Detail { get; set; } = new DeviceModel { };
        public bool IsDetailOpen { get; set; }

        public string MyProperty { get; set; }

        public override Task PreRender()
        {
            Devices.LoadFromQueryable(FakeDb.GetQueriable());
            return base.PreRender();
        }

        [AllowStaticCommand]
        public static DeviceModel Save(DeviceModel model)
        {
            if(model.Id == null || model.Id == Guid.Empty)
            {
                return FakeDb.Insert(model);
            }
            return FakeDb.Update(model);
        }

        [AllowStaticCommand]
        public static DeviceModel Blank() => new DeviceModel { };

        [AllowStaticCommand]
        public static IList<DeviceModel> List() => FakeDb.GetQueriable().ToList();

        [AllowStaticCommand]
        public static Task<DeviceModel> Get(Guid id) => Task.FromResult(FakeDb.Get(id));

        [AllowStaticCommand]
        public static void Remove(Guid id) => FakeDb.Remove(id);
    }
}

