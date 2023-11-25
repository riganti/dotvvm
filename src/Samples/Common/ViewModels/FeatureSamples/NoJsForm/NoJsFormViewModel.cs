using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.NoJsForm
{
    public class NoJsFormViewModel : DotvvmViewModelBase
    {
        public string Form1Value { get; set; }
        public string Form2Value { get; set; }

        public override async Task Load()
        {
            var req = Context.HttpContext.Request;
            if (req.Method == "POST")
            {
                using var body = new StreamReader(req.Body);
                var data = HttpUtility.ParseQueryString(await body.ReadToEndAsync());
                var submit = data["submit"];
                if (submit == "form1")
                {
                    Form1Value = data["text"];
                }
                else if (submit == "form2")
                {
                    Form2Value = data["text"];
                }
            }
        }
    }
}

