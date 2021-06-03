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
        [NoCache]
        public List<Company<string>> Get()
        {
            lock (Database.Instance)
            {
                return Database.Instance.Companies;
            }
        }

        [HttpGet]
        [Route("sorted")]
        [NoCache]
        public GridViewDataSet<Company<bool>> GetWithSorting([FromUri, AsObject(typeof(ISortingOptions))]SortingOptions sortingOptions)
        {
            lock (Database.Instance)
            {
                var dataSet = new GridViewDataSet<Company<bool>>()
                {
                    SortingOptions = sortingOptions
                };
                dataSet.LoadFromQueryable(Database.Instance.Companies2.AsQueryable());
                return dataSet;
            }
        }

        [HttpGet]
        [Route("paged")]
        [NoCache]
        public GridViewDataSet<Company<string>> GetWithPaging([FromUri, AsObject(typeof(IPagingOptions))]PagingOptions pagingOptions)
        {
            lock (Database.Instance)
            {
                var dataSet = new GridViewDataSet<Company<string>>()
                {
                    PagingOptions = pagingOptions
                };
                dataSet.LoadFromQueryable(Database.Instance.Companies.AsQueryable());
                return dataSet;
            }
        }

        [HttpGet]
        [Route("sortedandpaged")]
        [NoCache]
        public GridViewDataSet<Company<string>> GetWithSortingAndPaging([FromUri, AsObject(typeof(ISortingOptions))]SortingOptions sortingOptions, [FromUri, AsObject(typeof(IPagingOptions))]PagingOptions pagingOptions)
        {
            lock (Database.Instance)
            {
                var dataSet = new GridViewDataSet<Company<string>>()
                {
                    PagingOptions = pagingOptions,
                    SortingOptions = sortingOptions
                };
                dataSet.LoadFromQueryable(Database.Instance.Companies.AsQueryable());
                return dataSet;
            }
        }
    }
}
