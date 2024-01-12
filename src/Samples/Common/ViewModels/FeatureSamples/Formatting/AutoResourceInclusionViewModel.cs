using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Formatting
{
    public class AutoResourceInclusionViewModel : AutoResourceInclusionMasterViewModel
    {
        public List<Item> Items { get; set; }

        public int Number { get; set; } = 15;

        public override Task PreRender()
        {
            if (!Context.IsPostBack)
            {
                Items = new List<Item>()
                {
                    new Item() { Id = 1, DateCreated = new DateTime(2020, 1, 2, 20, 3, 45) },
                    new Item() { Id = 2, DateCreated = new DateTime(2020, 1, 2, 21, 4, 46) },
                    new Item() { Id = 3, DateCreated = new DateTime(2020, 1, 2, 22, 5, 47) },
                };
            }
            return base.PreRender();
        }

        public class Item
        {
            public int Id { get; set; }

            public DateTime DateCreated { get; set; }
        }

    }
}

