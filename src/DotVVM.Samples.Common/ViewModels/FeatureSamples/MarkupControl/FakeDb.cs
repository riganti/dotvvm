using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
    public static class FakeDb
    {
        public static DeviceModel Insert(DeviceModel model)
        {
            model.Id = Guid.NewGuid();
            devices.Add(model);
            return model;
        }
        public static DeviceModel Update(DeviceModel model)
        {
            var d = Get(model.Id);
            d.Name = model.Name;
            d.Groups = model.Groups;
            return d;
        }

        public static void Remove(Guid id) => devices = devices.Where(d => d.Id != id).ToList();
        public static DeviceModel Get(Guid id) => devices.FirstOrDefault(d=>d.Id == id);
        public static IQueryable<DeviceModel> GetQueriable() => devices.AsQueryable();

        private static IList<DeviceModel> devices = new List<DeviceModel> {
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

