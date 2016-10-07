using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.FileUploadInRepeater
{
    public class FileUploadInRepeaterViewModel : DotvvmViewModelBase
    {

        public List<FileUploadInRepeaterCollection> Collections { get; set; }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Collections = new List<FileUploadInRepeaterCollection>()
                {
                    new FileUploadInRepeaterCollection() { Id = Guid.NewGuid() },
                    new FileUploadInRepeaterCollection() { Id = Guid.NewGuid() },
                    new FileUploadInRepeaterCollection() { Id = Guid.NewGuid() }
                };
            }
            return base.Init();
        }
    }

    public class FileUploadInRepeaterCollection
    {

        public UploadedFilesCollection Files { get; set; } = new UploadedFilesCollection();

        public Guid Id { get; set; }

        public int FilesCount { get; set; }

    }
}