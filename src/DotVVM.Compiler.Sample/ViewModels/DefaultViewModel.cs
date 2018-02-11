using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Compiler.Sample.ViewModels;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Compiler.Sample.ViewModels
{
    public class DefaultViewModel : MasterPageViewModel
    {

        public string Value0 { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public InnerData Inner { get; set; }
        public DefaultViewModel()
        {
            Inner = new InnerData();
            
            Value0 = "Value 0";
            Value1 = "Value 1";
            Value2 = "Value 2";
            Value3 = "Value 3";
        }
    }

    public class InnerData
    {
        public string Value4 { get; set; }

        public InnerData()
        {
            Value4 = "Value 4";
        }
    }
}
