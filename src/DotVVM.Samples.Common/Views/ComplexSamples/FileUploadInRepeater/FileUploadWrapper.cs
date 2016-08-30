using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;

namespace DotVVM.Samples.BasicSamples.Views.ComplexSamples.FileUploadInRepeater
{
    public class FileUploadWrapper : DotvvmMarkupControl
    {

        public UploadedFilesCollection Files
        {
            get { return (UploadedFilesCollection)GetValue(FilesProperty); }
            set { SetValue(FilesProperty, value); }
        }
        public static readonly DotvvmProperty FilesProperty
            = DotvvmProperty.Register<UploadedFilesCollection, FileUploadWrapper>(c => c.Files, null);


        public int FilesCount
        {
            get { return (int)GetValue(FilesCountProperty); }
            set { SetValue(FilesCountProperty, value); }
        }
        public static readonly DotvvmProperty FilesCountProperty
            = DotvvmProperty.Register<int, FileUploadWrapper>(c => c.FilesCount);


        public void OnUploaded()
        {
            FilesCount++;
        }

    }
}