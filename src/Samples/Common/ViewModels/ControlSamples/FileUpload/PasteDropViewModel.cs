using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.FileUpload
{
	public class PasteDropViewModel : DotvvmViewModelBase
	{
		public string Text { get; set; }

        public int FilesCount { get; set; }

        public UploadedFilesCollection Files { get; set; } = new UploadedFilesCollection();


        public void OnUploadCompleted()
        {
            FilesCount++;
        }
    }
}

