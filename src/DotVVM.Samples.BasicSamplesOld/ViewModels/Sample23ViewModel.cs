using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.Binding;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample23ViewModel : DotvvmViewModelBase
    {

        public string NewTaskTitle { get; set; }

        public List<Task2ViewModel> Tasks { get; set; }

        public Sample23ViewModel()
        {
            Tasks = new List<Task2ViewModel>();
        }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Tasks.Add(new Task2ViewModel() { IsCompleted = false, TaskId = Guid.NewGuid(), Title = "Do the laundry" });
                Tasks.Add(new Task2ViewModel() { IsCompleted = true, TaskId = Guid.NewGuid(), Title = "Wash the car" });
                Tasks.Add(new Task2ViewModel() { IsCompleted = true, TaskId = Guid.NewGuid(), Title = "Go shopping" });
            }
            return base.Init();
        }

        public void AddTask()
        {
            Tasks.Add(new Task2ViewModel()
            {
                Title = NewTaskTitle,
                TaskId = Guid.NewGuid()
            });
            NewTaskTitle = string.Empty;
        }

        public void CompleteTask(Guid id)
        {
            Tasks.Single(t => t.TaskId == id).IsCompleted = true;
        }


        [StaticCommandCallable]
        public static void CoolAction(string newTaskTitle)
        {
            Debug.WriteLine(newTaskTitle);
        }
    }


    public class Task2ViewModel
    {

        public Guid TaskId { get; set; }

        public string Title { get; set; }

        public bool IsCompleted { get; set; }

        [StaticCommandCallable]
        public void PrintToDebug()
        {
            Debug.WriteLine(Title);
        }
    }
}