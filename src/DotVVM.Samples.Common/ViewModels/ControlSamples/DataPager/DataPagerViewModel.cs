using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.DataPager
{
    public class DataPagerViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<Data> DataSet { get; set; }

        public bool Enabled { get; set; } = true;

        public int ItemsInDatabaseCount { get; set; } = 2;

        public bool ShouldLoadAsync { get; set; }

        public override Task Init()
        {
            if (ShouldLoadAsync)
            {
                DataSet = GridViewDataSet.Create(GetDataAsync, pageSize: 3);
            }
            else
            {
                DataSet = GridViewDataSet.Create((options) => Task.FromResult(GetData(options)), pageSize: 3);
            }
            return base.Init();
        }

        public void Populate()
        {
            ItemsInDatabaseCount = 20;
            DataSet.RequestRefresh(forceRefresh: true);
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

        private GridViewDataSetLoadedData<Data> GetData(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions)
        {
            var queryable = FakeDB(ItemsInDatabaseCount);
            return queryable.GetDataFromQueryable(gridViewDataSetLoadOptions);
        }

        private async Task<GridViewDataSetLoadedData<Data>> GetDataAsync(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions)
        {
            var queryable = FakeDB(ItemsInDatabaseCount);
            return await Task.Run(() => queryable.GetDataFromQueryable(gridViewDataSetLoadOptions));
        }

        public class Data
        {
            public string Text { get; set; }
        }
    }
}