using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Storage;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample19ViewModel : DotvvmViewModelBase
    {
        public UploadedFilesCollection Files { get; set; }



        public List<string> FilesInStorage
        {
            get { return Directory.GetFiles(GetUploadPath()).Select(Path.GetFileName).ToList(); }
        }

        public Sample19ViewModel()
        {
            Files = new UploadedFilesCollection();
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
            var uploadPath = Path.Combine(Context.Configuration.ApplicationPhysicalPath, "Sample19Files");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            return uploadPath;
        }
    }
    
}