using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using Newtonsoft.Json;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class RenamedPrimaryKeyViewModel
    {
        public GridViewDataSet<SampleDto> Samples { get; set; } = new GridViewDataSet<SampleDto> {
            RowEditOptions = new RowEditOptions {
                PrimaryKeyPropertyName = nameof(SampleDto.Id)
            },
            Items =  {
                new SampleDto
                {
                    Id = "1",
                    Name = "One"
                },
                new SampleDto
                {
                    Id = "2",
                    Name = "Two"
                },
                new SampleDto
                {
                    Id = "3",
                    Name = "Three"
                }
            }
        };

        public void Edit(string id)
        {
            Samples.RowEditOptions.EditRowId = id;
        }

        public void Save()
        {
            Samples.RowEditOptions.EditRowId = null;
        }

        public class SampleDto
        {
            [JsonProperty("id", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public string Id { get; set; }

            public string Name { get; set; }
        }
    }
}
