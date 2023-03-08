using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCoreLatest.Controllers
{
    [Route("api/[controller]")]
    public class SlowController : Controller
    {

        [HttpPost]
        [Route("random-number")]
        public async Task<int> RandomNumber()
        {
            await Task.Delay(2000);
            return Random.Shared.Next(100);
        }

    }
}
