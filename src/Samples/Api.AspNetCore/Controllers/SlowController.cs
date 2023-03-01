using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    public class SlowController : Controller
    {
        
        private static readonly Random random = new();

        [HttpPost]
        [Route("random-number")]
        public async Task<int> RandomNumber()
        {
            await Task.Delay(2000);
            lock (random)
            {
                return random.Next(100);
            }
        }

    }
}
