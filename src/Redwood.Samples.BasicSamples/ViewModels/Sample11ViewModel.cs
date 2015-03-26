using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.Validation;
using Redwood.Framework.ViewModel;

namespace Redwood.Samples.BasicSamples.ViewModels
{
    public class Sample11ViewModel : RedwoodViewModelBase
    {

        [Required]
        public string NewTaskTitle { get; set; }

        public List<TaskViewModel> Tasks { get; set; }

        public Sample11ViewModel()
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

        public override IEnumerable<ValidationRule> GetRulesFor(Type t)
        {
            // in real application you will not hardcode rules like that but use some library
            // like `FluentValidation.Redwood.RuleConverter.GetRules(new MyViewModelValiudator())`
            if (t == typeof(TaskViewModel))
                return new ValidationRule[] {
                    ValidationRule.Create<TaskViewModel, string>(
                        tt => tt.Title,
                        "Title can not start with underscore",
                        "regularExpression",
                        new object[] { "^[^_]" },
                        c => !(((string)c.Value).StartsWith("_") || ((string)c.Value).StartsWith("^")),
                        "action:CompleteTask")
                };
            return base.GetRulesFor(t);
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

        [ValidationSettings(ValidateAll = false, DefineGroups = new[] { "action:CompleteTask" })]
        public void CompleteTask(Guid id)
        {
            Tasks.Single(t => t.TaskId == id).IsCompleted = true;
        }

    }
}