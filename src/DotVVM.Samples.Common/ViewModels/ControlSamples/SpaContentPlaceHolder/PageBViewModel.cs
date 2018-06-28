using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.TaskList;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.SpaContentPlaceHolder
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
            Context.RedirectToRoute("ControlSamples_SpaContentPlaceHolder_PageA", new { Id = 15 });
        }
    }
}

