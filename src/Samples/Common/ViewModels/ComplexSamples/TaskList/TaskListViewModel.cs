using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Binding;
using System.Diagnostics;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.TaskList
{
    public class TaskListViewModel : DotvvmViewModelBase
    {
        public string NewTaskTitle { get; set; }

        public List<TaskViewModel> Tasks { get; set; }

        public string NewTaskName { get; set; }

        public TaskListViewModel()
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

        public async Task SaveDataAsync()
        {
            await Task.Delay(100); // do complicated database operation and send avian carriers to all our datacenters
            Tasks.Add(new TaskViewModel { IsCompleted = false, TaskId = Guid.NewGuid(), Title = "Implement task storage" });
        }

        public void CompleteTask(Guid id)
        {
            Tasks.Single(t => t.TaskId == id).IsCompleted = true;
        }

        [AllowStaticCommand]
        public static TaskViewModel StaticCompleteTask(TaskViewModel task)
        {
            task.IsCompleted = true;
            return task;
        }
        [AllowStaticCommand]
        public static TaskViewModel CreateTask(string task)
        {
            return new TaskViewModel { TaskId = Guid.NewGuid(), Title = task };
        }

    }

    public class TaskViewModel
    {

        public Guid TaskId { get; set; }

        public string Title { get; set; }

        public bool IsCompleted { get; set; }
    }
}
