using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
    public class StaticCommandInMarkupControlCallingRegularCommandViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<DeviceModel> Devices { get; set; } = new GridViewDataSet<DeviceModel> {
            PagingOptions = new PagingOptions {
                PageSize = 10
            }
        };

        public DeviceModel Detail { get; set; } = new DeviceModel { };
        public bool IsDetailOpen { get; set; }

        private static FakeDb FakeDb { get; } = new FakeDb();

        public override Task PreRender()
        {
            Devices.LoadFromQueryable(FakeDb.GetQueriable());
            return base.PreRender();
        }

        public void Save()
        {
            if (Detail.Id == null || Detail.Id == Guid.Empty)
            {
                Detail = FakeDb.Insert(Detail);
            }
            Detail = FakeDb.Update(Detail);
        }

        public void Edit(Guid id)
        {
            Detail = FakeDb.Get(id);
        }

        public void Remove(Guid id)
        {
            FakeDb.Remove(id);
        }

        public void Blank()
        {
            Detail= new DeviceModel { };
        }
    }
}

