using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample22ViewModel : DotvvmViewModelBase
    {

        public List<PageData> Pages { get; set; }

        public override Task Init()
        {
            Pages = new List<PageData>()
            {
                new PageData() { Id = 1, Name = "Humphrey" },
                new PageData() { Id = 2, Name = "Jim" },
                new PageData() { Id = 3, Name = "Bernard" }
            };

            return base.Init();
        }


        public class PageData
        {
            public string Name { get; set; }

            public int Id { get; set; }
        }
    }
}