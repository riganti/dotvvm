using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public abstract class Sample48ViewModel : DotvvmViewModelBase
    {

        public List<Sample48Entry> Children { get; set; }

        public abstract string ChangedValue { get; }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Children = new List<Sample48Entry>()
                {
                    new Sample48Entry() { Name = "Entry 1" },
                    new Sample48Entry() { Name = "Entry 2" },
                    new Sample48Entry() { Name = "Entry 3" }
                };
            }
            return base.Init();
        }

        public string TestResult { get; set; }

        public void Test()
        {
            if (this is Sample48ViewModelA)
            {
                TestResult = ((Sample48ViewModelA) this).TestValueA + ((Sample48ViewModelA) this).TestValue;
            }
            else
            {
                TestResult = ((Sample48ViewModelB) this).TestValueB + ((Sample48ViewModelB) this).TestValue;
            }
        }
    }

    public class Sample48ViewModelA : Sample48ViewModel
    {
        public override string ChangedValue => "A";

        public string TestValueA { get; set; } = "Hello";

        public string TestValue { get; set; } = "1";

    }

    public class Sample48ViewModelB : Sample48ViewModel
    {

        public override string ChangedValue => "B";

        public string TestValueB { get; set; } = "World";

        public string TestValue { get; set; } = "2";

    }

    public class Sample48Entry
    {
        public string Name { get; set; }

    }
}