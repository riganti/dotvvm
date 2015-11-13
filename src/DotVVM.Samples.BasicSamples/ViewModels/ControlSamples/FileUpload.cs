using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Storage;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples
{
    public class FileUpload : DotvvmViewModelBase
    {
        public UploadedFilesCollection Files { get; set; } = new UploadedFilesCollection();



        public List<string> FilesInStorage
        {
            get { return Directory.GetFiles(GetUploadPath()).Select(Path.GetFileName).ToList(); }
        }

        public override Task Init()
        {
            if (Context.Query.ContainsKey("delete"))
            {
                File.Delete(Path.Combine(GetUploadPath(), Convert.ToString(Context.Query["delete"])));
            }

            return base.Init();
        }


        public void Process()
        {
            var storage = Context.Configuration.ServiceLocator.GetService<IUploadedFileStorage>();

            var uploadPath = GetUploadPath();

            foreach (var file in Files.Files)
            {
                storage.SaveAs(file.FileId, Path.Combine(uploadPath, file.FileId + ".bin"));
                storage.DeleteFile(file.FileId);
            }
            Files.Clear();
        }

        private string GetUploadPath()
        {
            var uploadPath = Path.Combine(Context.Configuration.ApplicationPhysicalPath, "Temp\\Upload");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            return uploadPath;
        }
    }
}