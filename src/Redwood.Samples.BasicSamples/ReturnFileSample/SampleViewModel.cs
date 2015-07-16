using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Microsoft.Owin;
using Redwood.Framework.ViewModel;

namespace Redwood.Samples.BasicSamples.ReturnFileSample
{
    public class SampleViewModel : RedwoodViewModelBase
    {
        public void ReturnFile1()
        {
            var file = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"ReturnFileSample\excel-file.xlsx");
            var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            using (Stream stream = new FileStream(file, FileMode.Open))
            {
                Context.ReturnFile(stream, Path.GetFileName(file), mimeType, new HeaderDictionary(new Dictionary<string, string[]>()));
            }
        }

        public void ReturnFile2()
        {
            var file = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"ReturnFileSample\word-file.docx");
            var mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            using (Stream stream = new FileStream(file, FileMode.Open))
            {
                Context.ReturnFile(stream, Path.GetFileName(file), mimeType, new HeaderDictionary(new Dictionary<string, string[]>()));
            }
        }
    }
}
