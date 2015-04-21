using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Redwood.Samples.VirtualDirectorySamples.ViewModels
{
    public class Sample2ViewModel : MasterViewModel
    {

        public string RandomString { get; set; }

        public void Generate()
        {
            RandomString = Guid.NewGuid().ToString();
        }

    }
}