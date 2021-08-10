using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPAViewModelReapplication
{
    public abstract class SPAViewModelReapplicationViewModel : DotvvmViewModelBase
    {


        public List<Entry> Children { get; set; }

        public abstract string ChangedValue { get; }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Children = new List<Entry>()
                {
                    new Entry() { Name = "Entry 1" },
                    new Entry() { Name = "Entry 2" },
                    new Entry() { Name = "Entry 3" }
                };
            }
            return base.Init();
        }

        public string TestResult { get; set; }

        public void Test()
        {
            if (this is ViewModelA)
            {
                TestResult = ((ViewModelA)this).TestValueA + ((ViewModelA)this).TestValue;
            }
            else
            {
                TestResult = ((ViewModelB)this).TestValueB + ((ViewModelB)this).TestValue;
            }
        }
    }

    public class ViewModelA : SPAViewModelReapplicationViewModel
    {
        public override string ChangedValue => "A";

        public string TestValueA { get; set; } = "Hello";

        public string TestValue { get; set; } = "1";

    }

    public class ViewModelB : SPAViewModelReapplicationViewModel
    {

        public override string ChangedValue => "B";

        public string TestValueB { get; set; } = "World";

        public string TestValue { get; set; } = "2";

    }

    public class Entry
    {
        public string Name { get; set; }

    }
}
