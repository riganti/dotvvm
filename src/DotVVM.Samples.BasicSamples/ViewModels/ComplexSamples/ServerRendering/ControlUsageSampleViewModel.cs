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

        public List<ArticleDTO> Articles { get; set; } = new List<ArticleDTO>()
        {
            new ArticleDTO { Id = 1, DateSubmitted = DateTime.Now, Message="a" },
            new ArticleDTO { Id = 2, DateSubmitted = DateTime.Now, Message="b" },
            new ArticleDTO { Id = 3, DateSubmitted = DateTime.Now, Message="c" },
            new ArticleDTO { Id = 4, DateSubmitted = DateTime.Now, Message="d" }
        };

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

        public void EmptyEdit()
        { }

        public void EditArticleClick(int id)
        {
            EditedArticle = Articles
                .Where(a => a.Id == id)
                .Select(a => new ArticleDTO { Id = a.Id, DateSubmitted = a.DateSubmitted, Message = a.Message })
                .SingleOrDefault();
        }
    }
}
