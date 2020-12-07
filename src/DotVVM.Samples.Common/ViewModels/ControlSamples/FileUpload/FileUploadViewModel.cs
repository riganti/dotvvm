using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Storage;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.FileUpload
{
    public class FileUploadViewModel : DotvvmViewModelBase
    {
        private readonly IUploadedFileStorage fileStorage;
        public FileUploadViewModel(IUploadedFileStorage fileStorage)
        {
            this.fileStorage = fileStorage;
        }

        public UploadedFilesCollection Files { get; set; } = new UploadedFilesCollection();

        public List<string> FilesInStorage
        {
            get { return Directory.GetFiles(GetUploadPath()).Select(Path.GetFileName).ToList(); }
        }

        public bool IsFileTypeAllowed { get; set; }

        public bool IsMaxSizeExceeded { get; set; }

        public FileSize FileSize { get; set; } = new FileSize();

        public override Task Init()
        {
            if (Context.Query.ContainsKey("delete"))
            {
                File.Delete(Path.Combine(GetUploadPath(), Convert.ToString(Context.Query["delete"])));
            }

            return base.Init();
        }

        public void CheckFile()
        {
            var file = Files.Files.Last();
            IsFileTypeAllowed = file.IsFileTypeAllowed;
            IsMaxSizeExceeded = file.IsMaxSizeExceeded;
            FileSize = file.FileSize;
        }

        public void Process()
        {
            var uploadPath = GetUploadPath();

            foreach (var file in Files.Files)
            {
                fileStorage.SaveAs(file.FileId, Path.Combine(uploadPath, file.FileId + ".bin"));
                fileStorage.DeleteFile(file.FileId);
            }
            Files.Clear();
        }

        private string GetUploadPath()
        {
            var uploadPath = Path.Combine(Context.Configuration.ApplicationPhysicalPath, "Temp/Upload");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            return uploadPath;
        }
    }
}
