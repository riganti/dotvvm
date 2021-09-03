using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPA;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.BindingVariables
{
    public class StaticCommandVariablesWithServiceViewModel : SiteViewModel
    {
        public string Message1 { get; set; }
        public string Message2 { get; set; }
        public MultipleMessagesDTO Data { get; set; } = new MultipleMessagesDTO();
    }
    public class MultipleMessagesWrapper
    {
        public MultipleMessagesDTO Data { get; set; }
    }
    public class MultipleMessagesDTO
    {
        public string Message1 { get; set; }
        public string Message2 { get; set; }

    }
    public class VariablesStaticCommand
    {
        [AllowStaticCommand]
        public Task<MultipleMessagesDTO> GetMessages()
        {
            // This method could just return DTO but the purpose is to test that ".Result" is translated correctly.
            return Task.FromResult(new MultipleMessagesDTO() {
                Message1 = "test1",
                Message2 = "test2"
            });
        }

        [AllowStaticCommand]
        public Task<string> GetMessage()
        {
            // This method could just return DTO but the purpose is to test that ".Result" is translated correctly.
            return Task.FromResult("test1");
        }

        [AllowStaticCommand]
        public Task<MultipleMessagesWrapper> GetData()
        {
            // This method could just return DTO but the purpose is to test that ".Result" is translated correctly.
            return Task.FromResult(new MultipleMessagesWrapper() {
                Data = new MultipleMessagesDTO() {
                    Message1 = "test1",
                    Message2 = "test2"
                }
            });
        }

    }

}

