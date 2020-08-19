using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using SampleApp1.Models;

namespace SampleApp1.ViewModels.Controls
{
    public class PageWithControlsViewModel : DotvvmViewModelBase
    {
        public CounterDTO MainCounter { get; set; } = new CounterDTO { Count = 10 };

        public string Name { get; set; } = "Bye";

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
                        Counter = new CounterDTO()
                        {
                            Count = 5
                        }
                    },
                    new CounterSectionDTO()
                    {
                        Name = "Section B",
                        Counter = new CounterDTO()
                        {
                            Count = 3
                        }
                    },
                    new CounterSectionDTO()
                    {
                        Name = "Section C",
                        Counter = new CounterDTO()
                        {
                            Count = 3
                        }
                    }
                };
            }
            return base.Init();
        }

        public void AddControlB()
        {
            Sections.Add(new CounterSectionDTO()
            {
                Name = $"Section {(char)((int)'A' + Sections.Count)}",
                Counter = new CounterDTO()
            });
        }

    }
}

