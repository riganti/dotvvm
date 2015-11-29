using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample47ViewModel : DotvvmViewModelBase
    {

        public List<Sample47Entry> Children { get; set; }

        public string ClickedChild { get; set; }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Children = new List<Sample47Entry>()
                {
                    new Sample47Entry()
                    {
                        Name = "Child 1",
                        Children = new List<Sample47Entry>()
                        {
                            new Sample47Entry() { Name = "Subchild 1" },
                            new Sample47Entry() { Name = "Subchild 2" },
                            new Sample47Entry() { Name = "Subchild 3" }
                        }
                    },
                    new Sample47Entry()
                    {
                        Name = "Child 2",
                        Children = new List<Sample47Entry>()
                        {
                            new Sample47Entry() { Name = "Subchild 1" },
                            new Sample47Entry() { Name = "Subchild 2" }
                        }
                    },
                    new Sample47Entry()
                    {
                        Name = "Child 3",
                        Children = new List<Sample47Entry>()
                        {
                            new Sample47Entry() { Name = "Subchild 1" }
                        }
                    }
                };    
            }

            return base.Init();
        }

        public void Click(string name, string name2)
        {
            ClickedChild = name + " " + name2;
        }

    }

    public class Sample47Entry
    {
        public string Name { get; set; }

        public List<Sample47Entry> Children { get; set; } 
    }
}