using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using DotVVM.Samples.BasicSamples.Api.Common.DataStore;

namespace DotVVM.Samples.BasicSamples.Api.Owin.Controllers
{
    [RoutePrefix("api/reset")]
    public class ResetController : ApiController
    {

        [HttpPost]
        [Route("reset")]
        public void ResetData()
        {
            Database.Instance.SeedData();
        }

    }
}
