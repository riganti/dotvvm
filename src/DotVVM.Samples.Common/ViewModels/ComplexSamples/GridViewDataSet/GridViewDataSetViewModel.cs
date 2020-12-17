using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.GridViewDataSet
{
    public class GridViewDataSetViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<GridViewData> GridData { get; set; }

        public GridViewDataSetViewModel()
        {
            GridData = new GridViewDataSet<GridViewData>()
            {
                Pager =
                {
                    PageSize = 10
                }
            };
        }

        public override Task PreRender()
        {
            var data = new List<GridViewData>()
            {
                new GridViewData()
                {
                    DataId = 1,
                    Value = "Test 1"
                },
                new GridViewData()
                {
                    DataId = 2,
                    Value = "Test 2"
                },
                new GridViewData()
                {
                    DataId = 3,
                    Value = "Test 3"
                }
            };

            if (GridData.IsRefreshRequired)
            {
                GridData.LoadFromQueryable(data.AsQueryable());
            }

            return base.PreRender();
        }
    }

    public class GridViewData
    {
        public int DataId { get; set; }

        public string Value { get; set; }
    }
}
