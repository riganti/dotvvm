using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using SampleApp1.Models;

namespace SampleApp1.ViewModels.Controls
{
    public class SameSelectorsPageViewModel : DotvvmViewModelBase
    {
        public string Name { get; set; } = "Filip";

        public List<CounterSectionDTO> Sections { get; set; } = new List<CounterSectionDTO>();

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Sections = new List<CounterSectionDTO>()
                {
                    new CounterSectionDTO()
                    {
                        Name = "Section A",
                        Language = "Czech",
                        Counter = new CounterDTO()
                        {
                            Count = 3
                        }
                    },
                    new CounterSectionDTO()
                    {
                        Name = "Section B",
                        Language = "English",
                        Counter = new CounterDTO()
                        {
                            Count = 3
                        }
                    }
                };
            }
            return base.Init();
        }
    }
}

