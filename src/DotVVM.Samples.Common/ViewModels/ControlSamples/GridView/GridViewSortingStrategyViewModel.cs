using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.GridView
{
    public class GridViewSortingStrategyViewModel : DotvvmViewModelBase
    {
        private static IQueryable<CountryData> GetData()
        {
            return new[]
            {
                new CountryData() { Name = "Czech Republic", Population = 10_708_981, Area = 77_240 },
                new CountryData() { Name = "Slovakia", Population = 5_459_642, Area = 48_088 },
                new CountryData() { Name = "Germany", Population = 83_783_942, Area = 348_560 },
                new CountryData() { Name = "Austria", Population = 9_006_398, Area = 82_409 },
                new CountryData() { Name = "Poland", Population = 37_846_611, Area = 306_230 },
            }.AsQueryable();
        }

        public GridViewDataSet<CountryData> CountriesDataSet { get; set; } = new GridViewDataSet<CountryData>()
        {
            SortingOptions = new SortingOptions()
            {
                SortingStrategy = new SortingStrategy()
                {
                    Strategy = (dataSet, expr) =>
                    {
                        var options = dataSet.SortingOptions;

                        if (expr == "Name")
                        {
                            options.SortExpression = expr;
                            options.SortDescending = false;
                        }
                        else if (options.SortExpression == expr)
                        {
                            options.SortDescending ^= true;
                        }
                        else
                        {
                            options.SortExpression = expr;
                            options.SortDescending = false;
                        }

                        (dataSet as IPageableGridViewDataSet)?.GoToFirstPage();
                    }
                }
            }
        };

        public override Task PreRender()
        {
            CountriesDataSet.LoadFromQueryable(GetData());

            return base.PreRender();
        }
    }

    public class CountryData
    {
        public string Name { get; set; }
        public int Population { get; set; }
        public int Area { get; set; }
    }
}
