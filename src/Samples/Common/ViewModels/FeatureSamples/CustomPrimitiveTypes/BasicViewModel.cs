using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Core.Storage;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Validation;
using Newtonsoft.Json;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    public class BasicViewModel : DotvvmViewModelBase
    {

        [FromRoute("id")]
        public SampleId IdInRoute { get; set; }

        [FromQuery("id")]
        public SampleId? IdInQuery { get; set; }

        [Required]
        public SampleId SelectedItemId { get; set; }

        [Required]
        public SampleId? SelectedItemNullableId { get; set; }

        public List<SampleItem> Items { get; set; } = new List<SampleItem>
        {
            new SampleItem() { Id = SampleId.CreateExisting(new Guid("96c37b99-5fd5-448c-8a64-977ae11b8b8b")), Text = "Item 1" },
            new SampleItem() { Id = SampleId.CreateExisting(new Guid("c2654a1f-3781-49a8-911b-c7346db166e0")), Text = "Item 2" },
            new SampleItem() { Id = SampleId.CreateExisting(new Guid("e467a201-9ab7-4cd5-adbf-66edd03f6ae1")), Text = "Item 3" },
        };

        public SampleId StaticCommandResult { get; set; }

        [AllowStaticCommand]
        public SampleId StaticCommandWithSampleId(SampleId? current)
        {
            if (!Items.Any(i => i.Id == current))
            {
                throw new Exception("The 'current' parameter didn't deserialize correctly.");
            }
            return SampleId.CreateExisting(new Guid("54162c7e-cdcc-4585-aa92-2e78be3f0c75"));
        }

        public void CommandWithSampleId(SampleId current)
        {
            if (!Items.Any(i => i.Id == current))
            {
                throw new Exception("The 'current' parameter didn't deserialize correctly.");
            }
            if (current == Items[0].Id)
            {
                this.AddModelError(vm => vm.SelectedItemId, "Valid property path");
                this.AddModelError(vm => vm.SelectedItemId.IdValue, "Invalid property path");
                Context.FailOnInvalidModelState();
            }
        }

    }
}

