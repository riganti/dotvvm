using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Api.Swashbuckle.Attributes;
using DotVVM.Framework.Controls;
using DotVVM.Samples.BasicSamples.Api.Common.DataStore;
using DotVVM.Samples.BasicSamples.Api.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCoreLatest.Controllers
{
    [Route("api/[controller]")]
    public class CompaniesController : Controller
    {
        [HttpGet]
        public List<Company<string>> Get()
        {
            lock (Database.Instance)
            {
                return Database.Instance.Companies;
            }
        }

        [HttpGet]
        [Route("sorted")]
        public GridViewDataSet<Company<bool>> GetWithSorting([FromQuery, AsObject(typeof(ISortingOptions))]DefaultGridSorter<Company<bool>> sortingOptions)
        {
            lock (Database.Instance)
            {
                var dataSet = new GridViewDataSet<Company<bool>>()
                {
                    Sorter = sortingOptions
                };
                dataSet.LoadFromQueryable(Database.Instance.Companies2.AsQueryable());
                return dataSet;
            }
        }

        [HttpGet]
        [Route("paged")]
        public GridViewDataSet<Company<string>> GetWithPaging([FromQuery, AsObject(typeof(IPagingOptions))] DefaultGridPager<Company<string>, DistanceNearPageIndexesProvider<Company<string>>> pagingOptions)
        {
            lock (Database.Instance)
            {
                var dataSet = new GridViewDataSet<Company<string>>()
                {
                    Pager = pagingOptions
                };
                dataSet.LoadFromQueryable(Database.Instance.Companies.AsQueryable());
                return dataSet;
            }
        }

        [HttpGet]
        [Route("sortedandpaged")]
        public GridViewDataSet<Company<string>> GetWithSortingAndPaging([FromQuery, AsObject(typeof(ISortingOptions))]DefaultGridSorter<Company<string>> sortingOptions, [FromQuery, AsObject(typeof(IPagingOptions))]DefaultGridPager<Company<string>, DistanceNearPageIndexesProvider<Company<string>>> pagingOptions)
        {
            lock (Database.Instance)
            {
                var dataSet = new GridViewDataSet<Company<string>>()
                {
                    Pager = pagingOptions,
                    Sorter = sortingOptions
                };
                dataSet.LoadFromQueryable(Database.Instance.Companies.AsQueryable());
                return dataSet;
            }
        }
    }
}
