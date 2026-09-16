using System.Collections.Generic;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JavascriptTranslation
{
    public class FirstOrDefaultViewModel : DotvvmViewModelBase
    {
        [FromQuery("variant")]
        public string Variant { get; set; } = "predicate";

        [FromQuery("key")]
        public string Key { get; set; } = "";

        public List<CategoryInfo> Categories1 { get; set; } = new List<CategoryInfo> {
            new CategoryInfo { Id = 1, Name = "C1-1" },
            new CategoryInfo { Id = 2, Name = "C1-2" },
            new CategoryInfo { Id = 3, Name = "C1-3" }
        };

        public List<CategoryInfo> Categories2 { get; set; } = new List<CategoryInfo> {
            new CategoryInfo { Id = 10, Name = "C2-10" },
            new CategoryInfo { Id = 11, Name = "C2-11" },
            new CategoryInfo { Id = 12, Name = "C2-12" }
        };

        public List<ProductInfo> Products { get; set; } = new List<ProductInfo> {
            new ProductInfo { Name = "Product 1", Category1 = 1, Category2 = 10, Colors = new List<string> { "Red" } },
            new ProductInfo { Name = "Product 2", Category1 = 2, Category2 = 11, Colors = new List<string> { "Green", "Yellow" } },
            new ProductInfo { Name = "Product 3", Category1 = 3, Category2 = 12, Colors = new List<string> { "Orange" } }
        };

        public List<SortInfo> DostupnaRazeni { get; set; } = new List<SortInfo> {
            new SortInfo { KlicRazeni = "name", PopisRazeni = "Name" },
            new SortInfo { KlicRazeni = "date", PopisRazeni = "Date" }
        };

        public class CategoryInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ProductInfo
        {
            public int Category1 { get; set; }
            public int Category2 { get; set; }
            public string Name { get; set; }
            public List<string> Colors { get; set; }
        }

        public class SortInfo
        {
            public string KlicRazeni { get; set; }
            public string PopisRazeni { get; set; }
        }
    }
}
