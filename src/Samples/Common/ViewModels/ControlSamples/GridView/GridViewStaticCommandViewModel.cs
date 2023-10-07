using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class GridViewStaticCommandViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<CustomerData> StandardDataSet { get; set; } = new() {
            PagingOptions = new PagingOptions
            {
                PageSize = 10
            }
        };

        public NextTokenGridViewDataSet NextTokenDataSet { get; set; } = new();

        public NextTokenHistoryGridViewDataSet NextTokenHistoryDataSet { get; set; } = new();

        public MultiSortGridViewDataSet MultiSortDataSet { get; set; } = new();

        private static IQueryable<CustomerData> GetData()
        {
            return new[]
            {
                new CustomerData {CustomerId = 1, Name = "John Doe", BirthDate = DateTime.Parse("1976-04-01")},
                new CustomerData {CustomerId = 2, Name = "John Deer", BirthDate = DateTime.Parse("1984-03-02")},
                new CustomerData {CustomerId = 3, Name = "Johnny Walker", BirthDate = DateTime.Parse("1934-01-03")},
                new CustomerData {CustomerId = 4, Name = "Jim Hacker", BirthDate = DateTime.Parse("1912-11-04")},
                new CustomerData {CustomerId = 5, Name = "Joe E. Brown", BirthDate = DateTime.Parse("1947-09-05")},
                new CustomerData {CustomerId = 6, Name = "Jack Daniels", BirthDate = DateTime.Parse("1956-07-06")},
                new CustomerData {CustomerId = 7, Name = "James Bond", BirthDate = DateTime.Parse("1965-05-07")},
                new CustomerData {CustomerId = 8, Name = "John Smith", BirthDate = DateTime.Parse("1974-03-08")},
                new CustomerData {CustomerId = 9, Name = "Jack & Jones", BirthDate = DateTime.Parse("1976-03-22")},
                new CustomerData {CustomerId = 10, Name = "Jim Bill", BirthDate = DateTime.Parse("1974-09-20")},
                new CustomerData {CustomerId = 11, Name = "James Joyce", BirthDate = DateTime.Parse("1982-11-28")},
                new CustomerData {CustomerId = 12, Name = "Joudy Jane", BirthDate = DateTime.Parse("1958-12-14")}
            }.AsQueryable();
        }

        public override Task PreRender()
        {
            // fill dataset
            if (!Context.IsPostBack)
            {
                StandardDataSet.LoadFromQueryable(GetData());
                NextTokenDataSet.LoadFromQueryable(GetData());
                NextTokenHistoryDataSet.LoadFromQueryable(GetData());
                MultiSortDataSet.LoadFromQueryable(GetData());
            }
            return base.PreRender();
        }

        [AllowStaticCommand]
        public async Task<GridViewDataSet<CustomerData>> LoadStandard(GridViewDataSetOptions options)
        {
            var dataSet = new GridViewDataSet<CustomerData>();
            dataSet.ApplyOptions(options);
            dataSet.LoadFromQueryable(GetData());
            return dataSet;
        }

        [AllowStaticCommand]
        public async Task<NextTokenGridViewDataSet> LoadToken(GridViewDataSetOptions<NoFilteringOptions, SortingOptions, CustomerDataNextTokenPagingOptions> options)
        {
            var dataSet = new NextTokenGridViewDataSet();
            dataSet.ApplyOptions(options);
            dataSet.LoadFromQueryable(GetData());
            return dataSet;
        }

        [AllowStaticCommand]
        public async Task<NextTokenHistoryGridViewDataSet> LoadTokenHistory(GridViewDataSetOptions<NoFilteringOptions, SortingOptions, CustomerDataNextTokenHistoryPagingOptions> options)
        {
            var dataSet = new NextTokenHistoryGridViewDataSet();
            dataSet.ApplyOptions(options);
            dataSet.LoadFromQueryable(GetData());
            return dataSet;
        }

        [AllowStaticCommand]
        public async Task<MultiSortGridViewDataSet> LoadMultiSort(GridViewDataSetOptions<NoFilteringOptions, MultiCriteriaSortingOptions, PagingOptions> options)
        {
            var dataSet = new MultiSortGridViewDataSet();
            dataSet.ApplyOptions(options);
            dataSet.LoadFromQueryable(GetData());
            return dataSet;
        }

        public class NextTokenGridViewDataSet : GenericGridViewDataSet<CustomerData, NoFilteringOptions, SortingOptions, CustomerDataNextTokenPagingOptions, RowInsertOptions<CustomerData>, RowEditOptions>
        {
            public NextTokenGridViewDataSet() : base(new NoFilteringOptions(), new SortingOptions(), new CustomerDataNextTokenPagingOptions(), new RowInsertOptions<CustomerData>(), new RowEditOptions())
            {
            }
        }

        public class CustomerDataNextTokenPagingOptions : NextTokenPagingOptions, IApplyToQueryable, IPagingOptionsLoadingPostProcessor
        {
            public IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable)
            {
                var token = int.Parse(CurrentToken ?? "0");

                return queryable.Cast<CustomerData>()
                    .OrderBy(c => c.CustomerId)
                    .Where(c => c.CustomerId > token)
                    .Take(3)
                    .Cast<T>();
            }

            public void ProcessLoadedItems<T>(IQueryable<T> filteredQueryable, IList<T> items)
            {
                var lastToken = items.Cast<CustomerData>()
                    .OrderByDescending(c => c.CustomerId)
                    .FirstOrDefault()?.CustomerId;

                NextPageToken = (lastToken ?? 0).ToString();
            }
        }

        public class NextTokenHistoryGridViewDataSet : GenericGridViewDataSet<CustomerData, NoFilteringOptions, SortingOptions, CustomerDataNextTokenHistoryPagingOptions, RowInsertOptions<CustomerData>, RowEditOptions>
        {
            public NextTokenHistoryGridViewDataSet() : base(new NoFilteringOptions(), new SortingOptions(), new CustomerDataNextTokenHistoryPagingOptions(), new RowInsertOptions<CustomerData>(), new RowEditOptions())
            {
            }
        }

        public class CustomerDataNextTokenHistoryPagingOptions : NextTokenHistoryPagingOptions, IApplyToQueryable, IPagingOptionsLoadingPostProcessor
        {
            public IQueryable<T> ApplyToQueryable<T>(IQueryable<T> queryable)
            {
                var token = PageIndex < TokenHistory.Count - 1 ? int.Parse(TokenHistory[PageIndex - 1] ?? "0") : 0;

                return queryable.Cast<CustomerData>()
                    .OrderBy(c => c.CustomerId)
                    .Where(c => c.CustomerId > token)
                    .Take(3)
                    .Cast<T>();
            }

            public void ProcessLoadedItems<T>(IQueryable<T> filteredQueryable, IList<T> items)
            {
                if (PageIndex == TokenHistory.Count)
                {
                    var lastToken = items.Cast<CustomerData>()
                        .OrderByDescending(c => c.CustomerId)
                        .FirstOrDefault()?.CustomerId;

                    TokenHistory.Add((lastToken ?? 0).ToString());
                }
            }
        }

        public class MultiSortGridViewDataSet : GenericGridViewDataSet<CustomerData, NoFilteringOptions, MultiCriteriaSortingOptions, PagingOptions, RowInsertOptions<CustomerData>, RowEditOptions>
        {
            public MultiSortGridViewDataSet() : base(new NoFilteringOptions(), new MultiCriteriaSortingOptions(), new PagingOptions(), new RowInsertOptions<CustomerData>(), new RowEditOptions())
            {
            }
        }
    }
}
