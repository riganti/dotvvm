using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.TaskList;
using DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.SpaContentPlaceHolder;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.SpaContentPlaceHolder_PrefixRouteName
{
    public class PageBViewModel : SpaMasterViewModel
	{
        public TaskListViewModel Sample1 { get; set; } = new TaskListViewModel();

        public PageBViewModel()
        {
            HeaderText = "Task List";
        }

        public void Redirect()
        {
            Context.RedirectToRoute("ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageA", new { Id = 15 });
        }
    }
}

