using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ViewModelProtection
{
    public class ViewModelProtectionViewModel : DotvvmViewModelBase
    {
        [Protect(ProtectMode.EncryptData)]
        public string SecretData { get; set; }

        [Protect(ProtectMode.SignData)]
        public string ReadOnlyData { get; set; }


        public string NewTaskTitle { get; set; }

        public List<ProtectionTaskViewModel> ProtectedTasks { get; set; }

        public ViewModelProtectionViewModel()
        {
            ProtectedTasks = new List<ProtectionTaskViewModel>();
        }


        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                ReadOnlyData = "This property can be read on the client but when you modify it, the server won't accept the request.";
                SecretData = "This is encrypted and cannot be displayed on the client.";
                ProtectedTasks.Add(new ProtectionTaskViewModel() { IsCompleted = false, TaskId = Guid.NewGuid(), Title = "Do the laundry" });
                ProtectedTasks.Add(new ProtectionTaskViewModel() { IsCompleted = true, TaskId = Guid.NewGuid(), Title = "Wash the car" });
                ProtectedTasks.Add(new ProtectionTaskViewModel() { IsCompleted = true, TaskId = Guid.NewGuid(), Title = "Go shopping" });
            }
            return base.Init();
        }

        public void AddTask()
        {
            ValidateEncryptedData();

            ProtectedTasks.Add(new ProtectionTaskViewModel()
            {
                Title = NewTaskTitle,
                TaskId = Guid.NewGuid()
            });
            NewTaskTitle = string.Empty;
        }

        public void CompleteTask(Guid id)
        {
            ValidateEncryptedData();

            ProtectedTasks.Single(t => t.TaskId == id).IsCompleted = true;
        }

        private void ValidateEncryptedData()
        {
            if (SecretData != "This is encrypted and cannot be displayed on the client.")
            {
                throw new Exception("Security error!");
            }
        }

    }

    public class ProtectionTaskViewModel
    {

        public Guid TaskId { get; set; }

        public string Title { get; set; }

        public bool IsCompleted { get; set; }

        [Protect(ProtectMode.EncryptData)]
        public string SecretTaskData { get; set; }

        public ProtectionTaskViewModel()
        {
            SecretTaskData = "secret task data";
        }
    }
}