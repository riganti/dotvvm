using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using SampleApp1.Models;

namespace SampleApp1.ViewModels.MasterPages
{
    public class PageBViewModel : SiteViewModel
    {

        public NewTaskDTO NewTask { get; set; } = new NewTaskDTO();

        public List<TaskDTO> Tasks { get; set; } = new List<TaskDTO>();

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Tasks.Add(new TaskDTO { Text = "Go shopping", IsCompleted = true });
                Tasks.Add(new TaskDTO { Text = "Walk the dog", IsCompleted = false });
                Tasks.Add(new TaskDTO { Text = "Send letter", IsCompleted = false });
            }

            return base.Init();
        }

        public void AddTask()
        {
            Tasks.Add(new TaskDTO() { Text = NewTask.Text });
            NewTask = new NewTaskDTO();
        }

        public void SetCompleted(TaskDTO task)
        {
            task.IsCompleted = true;
        }

    }
}

