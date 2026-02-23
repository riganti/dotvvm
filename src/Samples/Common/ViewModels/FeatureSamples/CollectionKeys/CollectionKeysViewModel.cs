using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CollectionKeys
{
    public class CollectionKeysViewModel : DotvvmViewModelBase
    {
        public string NewTaskTitle { get; set; }

        public List<TaskViewModel> Tasks { get; set; }

        public CollectionKeysViewModel()
        {
            Tasks = new List<TaskViewModel>();
        }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Tasks.Add(new TaskViewModel() { IsCompleted = false, TaskId = Guid.NewGuid(), Title = "Do the laundry" });
                Tasks.Add(new TaskViewModel() { IsCompleted = false, TaskId = Guid.NewGuid(), Title = "Wash the car" });
                Tasks.Add(new TaskViewModel() { IsCompleted = false, TaskId = Guid.NewGuid(), Title = "Go shopping" });
            }

            return base.Init();
        }

        public void AddTask()
        {
            Tasks.Add(new TaskViewModel() { Title = NewTaskTitle, TaskId = Guid.NewGuid() });
            NewTaskTitle = string.Empty;
        }

        public void CompleteTask(Guid id)
        {
            Tasks.RemoveAll(t => t.TaskId == id);
        }

    }

    public class TaskViewModel
    {
        [Key]
        public Guid TaskId { get; set; }

        public string Title { get; set; }

        public bool IsCompleted { get; set; }
    }

}
