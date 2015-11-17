using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.Repeater
{
    public class NestedRepeaterViewModel : DotvvmViewModelBase
    {

        public List<NestedRepeaterEntry> Children { get; set; }

        public string ClickedChild { get; set; }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Children = new List<NestedRepeaterEntry>()
                {
                    new NestedRepeaterEntry()
                    {
                        Name = "Child 1",
                        Children = new List<NestedRepeaterEntry>()
                        {
                            new NestedRepeaterEntry() { Name = "Subchild 1" },
                            new NestedRepeaterEntry() { Name = "Subchild 2" },
                            new NestedRepeaterEntry() { Name = "Subchild 3" }
                        }
                    },
                    new NestedRepeaterEntry()
                    {
                        Name = "Child 2",
                        Children = new List<NestedRepeaterEntry>()
                        {
                            new NestedRepeaterEntry() { Name = "Subchild 1" },
                            new NestedRepeaterEntry() { Name = "Subchild 2" }
                        }
                    },
                    new NestedRepeaterEntry()
                    {
                        Name = "Child 3",
                        Children = new List<NestedRepeaterEntry>()
                        {
                            new NestedRepeaterEntry() { Name = "Subchild 1" }
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

    public class NestedRepeaterEntry
    {
        public string Name { get; set; }

        public List<NestedRepeaterEntry> Children { get; set; } 
    }
}