using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.DataPager
{
    public class DataPagerViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<Data> DataSet { get; set; }
        public override Task Init()
        {
            DataSet = GridViewDataSet.Create(GetData, pageSize: 3);
            return base.Init();
        }


        public bool Enabled { get; set; } = true;

        public int ItemsInDatabaseCount { get; set; } = 2;

        public void Populate()
        {
            ItemsInDatabaseCount = 20;
            DataSet.RequestRefresh(forceRefresh: true);
        }

        private GridViewDataSetLoadedData<Data> GetData(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions)
        {
            var queryable = FakeDB(ItemsInDatabaseCount);
            return queryable.GetDataFromQueryable(gridViewDataSetLoadOptions);
        }

        private IQueryable<Data> FakeDB(int itemsCreatorCounter)
        {
            var dbdata = new List<Data>();
            for (var i = 0; i < itemsCreatorCounter; i++)
            {
                dbdata.Add(new Data
                {
                    Text = $"Item {i}"
                });
            }
            return dbdata.AsQueryable();
        }

    }

    public class Data
    {
        public string Text { get; set; }
    }
}