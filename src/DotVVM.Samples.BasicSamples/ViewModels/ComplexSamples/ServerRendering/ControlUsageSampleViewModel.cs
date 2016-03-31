using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.ServerRendering
{
    public class ControlUsageSampleViewModel : DotvvmViewModelBase
    {
        public bool ShowEditForm => EditedArticle != null;

        public bool ShowFirstForm { get; set; } = true;

        public ArticleDTO EditedArticle { get; set; } = null;

        public void ShowFormClick()
        {
            EditedArticle = new ArticleDTO
            {
                DateSubmitted = DateTime.UtcNow,
                Message = "<<<<<a>>>"
            };
        }

        public void RewriteArticle()
        {
            ShowFirstForm = false;
            EditedArticle = new ArticleDTO
            {
                DateSubmitted = DateTime.Today,
                Message = "<<<<b"
            };
        }
    }
}
