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
        public List<Company> Get()
        {
            lock (Database.Instance)
            {
                return Database.Instance.Companies;
            }
        }

        [HttpGet]
        [Route("sorted")]
        public GridViewDataSet<Company> GetWithSorting([FromUri, AsObject(typeof(ISortingOptions))]SortingOptions sortingOptions)
        {
            lock (Database.Instance)
            {
                var dataSet = new GridViewDataSet<Company>()
                {
                    SortingOptions = sortingOptions
                };
                dataSet.LoadFromQueryable(Database.Instance.Companies.AsQueryable());
                return dataSet;
            }
        }

        [HttpGet]
        [Route("paged")]
        public GridViewDataSet<Company> GetWithPaging([FromUri, AsObject(typeof(IPagingOptions))]PagingOptions pagingOptions)
        {
            lock (Database.Instance)
            {
                var dataSet = new GridViewDataSet<Company>()
                {
                    PagingOptions = pagingOptions
                };
                dataSet.LoadFromQueryable(Database.Instance.Companies.AsQueryable());
                return dataSet;
            }
        }

        [HttpGet]
        [Route("sortedandpaged")]
        public GridViewDataSet<Company> GetWithSortingAndPaging([FromUri, AsObject(typeof(ISortingOptions))]SortingOptions sortingOptions, [FromUri, AsObject(typeof(IPagingOptions))]PagingOptions pagingOptions)
        {
            lock (Database.Instance)
            {
                var dataSet = new GridViewDataSet<Company>()
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
