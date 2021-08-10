using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ReturnedFile
{
    public class ReturnedFileSampleViewModel: DotvvmViewModelBase
    {
        public string Text { get; set; } = "";
        public void GetFile()
        {
            Context.ReturnFile(Encoding.UTF8.GetBytes(Text), "file.txt", "text/plain");
        }
        public void GetFileInline()
        {
            Context.ReturnFile(Encoding.UTF8.GetBytes(Text), "file.txt", "text/plain", attachmentDispositionType: "inline");
        }
    }
}
