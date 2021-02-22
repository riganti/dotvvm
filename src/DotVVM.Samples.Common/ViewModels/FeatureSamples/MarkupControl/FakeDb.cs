using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
    public class FakeDb
    {
        public FakeDb()
        {
            Reset();
        }
        public DeviceModel Insert(DeviceModel model)
        {
            model.Id = Guid.NewGuid();
            devices.Add(model);
            return model;
        }
        public DeviceModel Update(DeviceModel model)
        {
            var d = Get(model.Id);
            d.Name = model.Name;
            d.Groups = model.Groups;
            return d;
        }

        public void Remove(Guid id) => devices = devices.Where(d => d.Id != id).ToList();
        public DeviceModel Get(Guid id) => devices.FirstOrDefault(d => d.Id == id);
        public IQueryable<DeviceModel> GetQueriable() => devices.AsQueryable();
        public void Reset()
        {
            devices = GetDevices();
        }

        private IList<DeviceModel> devices;

        private IList<DeviceModel> GetDevices() =>
            new List<DeviceModel> {
            new DeviceModel {
                Name = "Washing machine",
                Id = Guid.NewGuid(),
                Groups = new List<string> { "Laundry room" }
            },
            new DeviceModel {
                Name = "Stove",
                Id = Guid.NewGuid(),
                Groups = new List<string> { "Kitchen" }
            },
            new DeviceModel {
                Name = "Dryer",
                Id = Guid.NewGuid(),
                Groups = new List<string> { "Laundry room" }
            },
        };
    }
}

