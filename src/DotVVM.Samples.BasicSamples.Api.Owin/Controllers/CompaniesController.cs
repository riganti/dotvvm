using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DotVVM.Framework.Api.Swashbuckle.Attributes;
using DotVVM.Framework.Controls;
using DotVVM.Samples.BasicSamples.Api.Common.DataStore;
using DotVVM.Samples.BasicSamples.Api.Common.Model;

namespace DotVVM.Samples.BasicSamples.Api.Owin.Controllers
{
    [RoutePrefix("api/companies")]
    public class CompaniesController : ApiController
    {
        [HttpGet]
        [Route("")]
        public List<Company<string>> Get()
        {
            lock (Database.Instance)
            {
                return Database.Instance.Companies;
            }
        }

        [HttpGet]
        [Route("sorted")]
        public GridViewDataSet<Company<bool>> GetWithSorting([FromUri, AsObject(typeof(ISortingOptions))]DefaultGridSorter<Company<bool>> sortingOptions)
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
        public GridViewDataSet<Company<string>> GetWithPaging([FromUri, AsObject(typeof(IPagingOptions))]DefaultGridPager<Company<string>, DistanceNearPageIndexesProvider<Company<string>>> pagingOptions)
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
        public GridViewDataSet<Company<string>> GetWithSortingAndPaging([FromUri, AsObject(typeof(ISortingOptions))]DefaultGridSorter<Company<string>> sortingOptions, [FromUri, AsObject(typeof(IPagingOptions))] DefaultGridPager<Company<string>, DistanceNearPageIndexesProvider<Company<string>>> pagingOptions)
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
