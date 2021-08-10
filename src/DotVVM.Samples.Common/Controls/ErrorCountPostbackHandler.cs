using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class ErrorCountPostbackHandler : PostBackHandler
    {
        protected override string ClientHandlerName { get; } = "errorCount";

        public string ResultId
        {
            get { return (string)GetValue(ResultIdProperty); }
            set { SetValue(ResultIdProperty, value); }
        }
        public static readonly DotvvmProperty ResultIdProperty
            = DotvvmProperty.Register<string, ErrorCountPostbackHandler>(c => c.ResultId, null);

        protected override Dictionary<string, object> GetHandlerOptions()
        {
            return new Dictionary<string, object> {
                ["resultId"] = GetValueRaw(ResultIdProperty)
            };
        }
    }
}
