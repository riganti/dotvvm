using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.TaskList;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class SimpleValidationViewModel : DotvvmViewModelBase 
    {
        
        [Required]
        [EmailAddress]
        public string NewTaskTitle { get; set; }

        public List<TaskViewModel> Tasks { get; set; }

        public SimpleValidationViewModel()
        {
            Tasks = new List<TaskViewModel>();
        }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Tasks.Add(new TaskViewModel() { IsCompleted = false, TaskId = Guid.NewGuid(), Title = "Do the laundry" });
                Tasks.Add(new TaskViewModel() { IsCompleted = true, TaskId = Guid.NewGuid(), Title = "Wash the car" });
                Tasks.Add(new TaskViewModel() { IsCompleted = true, TaskId = Guid.NewGuid(), Title = "Go shopping" });
            }
            return base.Init();
        }
        
        public void AddTask()
        {
            Tasks.Add(new TaskViewModel() 
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

    }
}