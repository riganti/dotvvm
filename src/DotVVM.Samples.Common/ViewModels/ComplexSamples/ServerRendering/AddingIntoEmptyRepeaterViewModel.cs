using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.ServerRendering
{
    public class AddingIntoEmptyRepeaterViewModel : DotvvmViewModelBase
    {
        public List<ArticleDTO> EmptyArticles { get; set; } = new List<ArticleDTO>();
        public List<ArticleDTO> NonEmptyArticles { get; set; } = new List<ArticleDTO> { new ArticleDTO { Id = 1, Message= $"NonEmptyArticles 1", DateSubmitted= DateTime.UtcNow } };

        public void AddArticle()
        {
            EmptyArticles.Add(new ArticleDTO { Id = EmptyArticles.Count + 1, DateSubmitted = DateTime.UtcNow, Message = $"EmptyArticles {EmptyArticles.Count + 1}" });
            NonEmptyArticles.Add(new ArticleDTO { Id = NonEmptyArticles.Count + 1, DateSubmitted = DateTime.UtcNow, Message = $"NonEmptyArticles {NonEmptyArticles.Count + 1}" });
        }

        public void EditArticle(int Id)
        {
            //Not important
        }
    }
}

